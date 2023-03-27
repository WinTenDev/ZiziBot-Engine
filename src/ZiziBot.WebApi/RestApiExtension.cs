using System.Net;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ZiziBot.WebApi;

public static class RestApiExtension
{
    public static IServiceCollection ConfigureApi(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var jwtConfig = serviceProvider.GetRequiredService<IOptions<JwtConfig>>().Value;

        services.AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssemblyContaining<PostGlobalBanApiValidator>(ServiceLifetime.Transient);

        services
            .Configure<ApiBehaviorOptions>(options => { options.SuppressInferBindingSourcesForParameters = true; })
            .AddControllers(options => {
                    options.Conventions.Add(new ControllerHidingConvention());
                    options.Conventions.Add(new ActionHidingConvention());
                    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
                }
            )
            .AddNewtonsoftJson()
            .ConfigureApiBehaviorOptions(options => {
                options.InvalidModelStateResponseFactory = context => {
                    var transactionId = context.HttpContext.Request.Headers[HeaderKey.TransactionId].FirstOrDefault();

                    var errorDetails = context.ModelState
                        .Where(entry => entry.Value?.ValidationState == ModelValidationState.Invalid)
                        .Select(key => new
                        {
                            Id = key.Key,
                            Field = key.Key.Split('.').Last(),
                            Message = key.Value?.Errors.Select(e => e.ErrorMessage)
                        }).ToList();

                    var errors = errorDetails.SelectMany(x => x.Message).ToList();

                    return new BadRequestObjectResult(new ApiResponseBase<object>()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        transactionId = transactionId,
                        Message = "Please ensure your request",
                        Result = new
                        {
                            Error = errors.Aggregate((a, b) => $"{a}\n{b}"),
                            Errors = errors,
                            ErrorDetails = errorDetails,
                        }
                    });
                };
            });

        services.AddAuthorization()
            .AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o => {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true
                };

                o.Events = new JwtBearerEvents
                {
                    OnChallenge = async context => {
                        context.HandleResponse();

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(
                            new ApiResponseBase<bool>()
                            {
                                StatusCode = HttpStatusCode.Unauthorized,
                                Message = "Please ensure you have a valid token"
                            }
                        );
                    }
                };
            });

        services.ConfigureRateLimiter();

        return services;
    }

    public static IServiceCollection AddAllMiddleware(this IServiceCollection services)
    {
        services.Scan(
            selector => {
                selector.FromAssembliesOf(typeof(HeaderCheckMiddleware))
                    .AddClasses(filter => filter.InNamespaceOf<HeaderCheckMiddleware>())
                    .AsSelf()
                    .WithTransientLifetime();
            }
        );

        return services;
    }

    public static IApplicationBuilder UseAllMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<HeaderCheckMiddleware>();
        app.UseMiddleware<InjectHeaderMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.ConfigureRateLimiter();

        return app;
    }
}