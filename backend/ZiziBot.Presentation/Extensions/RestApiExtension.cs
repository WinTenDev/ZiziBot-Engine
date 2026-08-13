using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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
using ZiziBot.Application.Features.LoggerSink;
using ZiziBot.Application.Infrastructure.Config;
using ZiziBot.Application.Common.Converters.SystemTextJson;
using ZiziBot.Application.Core.Interfaces;
using ZiziBot.Presentation.Infrastructure.Http;
using ZiziBot.Presentation.Infrastructure.Middleware;
using ZiziBot.Presentation.Infrastructure.Mvc;
using ZiziBot.Presentation.Infrastructure.Swagger;

namespace ZiziBot.Presentation.Extensions;

public static class RestApiExtension
{
    private const string ALLOW_ANY_ORIGIN_POLICY = "AllowAnyOriginPolicy";

    public static IServiceCollection AddRestApi(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssemblyContaining<PostGlobalBanApiValidator>();

        services
            .Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressInferBindingSourcesForParameters = true;
            })
            .AddControllers(options =>
                {
                    options.Conventions.Add(new ControllerHidingConvention());
                    options.Conventions.Add(new ActionHidingConvention());
                    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
                }
            )
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var transactionId = context.HttpContext.GetTransactionId();

                    var errorDetails = context.ModelState
                        .Where(entry => entry.Value?.ValidationState == ModelValidationState.Invalid)
                        .Where(x => x.Key != "request")
                        .Select(key =>
                        {
                            var field = key.Key.Split('.').LastOrDefault() ?? "";

                            return new
                            {
                                Id = key.Key,
                                Field = field,
                                Message = key.Value?.Errors.Select(e => HumanizeError(e.ErrorMessage, field))
                            };
                        }).ToList();

                    var errors = errorDetails.SelectMany(x => x.Message ?? []).ToList();

                    return new BadRequestObjectResult(new ApiResponseBase<object>()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        TransactionId = transactionId,
                        Message = errors.Aggregate((a, b) => $"{a}\n{b}"),
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
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtConfig>>((options, jwtConfigAccessor) =>
            {
                var jwtConfig = jwtConfigAccessor.Value;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
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

        services.AddCorsConfiguration();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
            {
                options.DocumentFilter<HidePathDocumentFilter>();
                options.SchemaFilter<SwaggerIgnoreFilter>();
            }
        );

        services.ConfigureRateLimiter();

        services.AddHttpContextAccessor();
        services.AddScoped<IHttpContextHelper, HttpContextHelper>();

        return services;
    }

    private static string HumanizeError(string errorMessage, string fieldName)
    {
        if (errorMessage.Contains("could not be converted"))
        {
            var match = Regex.Match(errorMessage, @"to System\.(\w+)");
            var type = match.Success ? match.Groups[1].Value : "the correct format";

            return $"The value for '{fieldName}' is not valid. Expected a valid {type}";
        }

        if (errorMessage.Contains("is required"))
        {
            return $"The '{fieldName}' field is required.";
        }

        return errorMessage;
    }

    private static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(ALLOW_ANY_ORIGIN_POLICY, builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });

        return services;
    }

    public static IServiceCollection AddAllMiddleware(this IServiceCollection services)
    {
        services.Scan(selector =>
            {
                selector.FromAssembliesOf(typeof(GlobalExceptionMiddleware))
                    .AddClasses(filter => filter.InNamespaceOf<GlobalExceptionMiddleware>())
                    .AsSelf()
                    .WithScopedLifetime();
            }
        );

        return services;
    }

    public static WebApplication ConfigureApi(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestBodyGuardMiddleware>();
        app.UseMiddleware<InjectRequestMiddleware>();

        app.MapHub<LogHub>("/api/logging");

        app.MapControllers();

        app.UseRouting();
        app.UseCors(ALLOW_ANY_ORIGIN_POLICY);
        app.UseAuthentication();
        app.UseAuthorization();

        app.ConfigureRateLimiter();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.DefaultModelsExpandDepth(-1);
        });

        return app;
    }
}