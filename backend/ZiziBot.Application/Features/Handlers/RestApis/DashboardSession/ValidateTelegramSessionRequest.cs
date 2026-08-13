using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ZiziBot.Application.Common.Dtos;
using ZiziBot.Application.Common.Types;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.RestApis.DashboardSession;

public class ValidateTelegramSessionRequest : ApiPostRequestBase<TelegramSessionDto, ValidateDashboardSessionIdResponseDto>
{
}

public class ValidateDashboardSessionIdResponseDto
{
    public bool IsSessionValid { get; set; }
    public string BearerToken { get; set; }
}

public class ValidateTelegramSessionHandler(
    ILogger<ValidateTelegramSessionHandler> logger,
    DataFacade dataFacade
)
    : IApiRequestHandler<ValidateTelegramSessionRequest, ValidateDashboardSessionIdResponseDto>
{
    public async Task<ApiResponseBase<ValidateDashboardSessionIdResponseDto>> Handle(
        ValidateTelegramSessionRequest request,
        CancellationToken cancellationToken
    )
    {
        ApiResponseBase<ValidateDashboardSessionIdResponseDto> response = new();

        var botSetting = await dataFacade.MongoDb.BotSettings
            .Where(entity => entity.Status == EventStatus.Complete)
            .Where(entity => entity.Name == "Main")
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (botSetting == null)
        {
            return response.BadRequest("Suatu kesalahan terjadi, silahkan hubungi admin");
        }

        LoginWidget loginWidget = new(botSetting.Token);

        var sessionData = request.Body.ToDictionary(StringType.SnakeCase)
            .Select(pair => new KeyValuePair<string, string>(pair.Key.Replace(" ", "_"), pair.Value.ToString() ?? string.Empty))
            .Where(pair => pair.Value.IsNotNullOrEmpty())
            .Where(pair => pair.Key != "session_id");

        var checkAuthorization = loginWidget.CheckAuthorization(sessionData);

        if (checkAuthorization != WebAuthorization.Valid)
        {
            logger.LogDebug("Session is not valid for SessionId: {SessionId}. Result: {Result}", request.Body.SessionId, checkAuthorization);

            return response.Unauthorized($"Session is invalid. please send '/console' on Chat for create new session");
        }

        var dashboardSession = await dataFacade.MongoDb.DashboardSessions
            .Where(entity => entity.TelegramUserId == request.Body.Id)
            .Where(entity => entity.SessionId == request.Body.SessionId)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var stringToken = string.Empty;

        var sessionExpireDate = DateTime.UtcNow.AddDays(3);

        if (dashboardSession == null)
        {
            logger.LogDebug("Creating a new Console session for UserId {UserId}", request.Body.Id);
            var jwtConfig = await dataFacade.AppSetting.GetConfigSectionAsync<JwtConfig>();

            if (jwtConfig == null)
            {
                return response.BadRequest("Authentication is not yet configured");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim(RequestKey.UserId, request.Body.Id.ToString()),
            };

            var token = new JwtSecurityToken(jwtConfig.Issuer, jwtConfig.Audience, claims, expires: DateTime.Now.AddMinutes(15), signingCredentials: credentials);

            stringToken = new JwtSecurityTokenHandler().WriteToken(token);

            dataFacade.MongoDb.DashboardSessions.Add(new DashboardSessionEntity() {
                TelegramUserId = request.Body.Id,
                FirstName = request.Body.FirstName,
                LastName = request.Body.LastName,
                PhotoUrl = request.Body.PhotoUrl,
                Username = request.Body.Username,
                AuthDate = request.Body.AuthDate,
                Hash = request.Body.Hash,
                SessionId = request.Body.SessionId,
                BearerToken = stringToken,
                ExpireDate = sessionExpireDate,
                Status = EventStatus.Complete
            });
        }
        else
        {
            logger.LogDebug("Updating Console session for UserId {UserId}", dashboardSession.TelegramUserId);

            dashboardSession.ExpireDate = sessionExpireDate;
            stringToken = dashboardSession.BearerToken;
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return response.Success("Session saved successfully", new ValidateDashboardSessionIdResponseDto() {
            IsSessionValid = true,
            BearerToken = stringToken
        });
    }
}
