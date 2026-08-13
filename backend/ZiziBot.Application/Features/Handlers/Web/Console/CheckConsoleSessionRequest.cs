using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ZiziBot.Application.Common.Dtos;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Features.Handlers.Web.Console;

public class CheckConsoleSessionRequest : WebRequestBase<CheckConsoleSessionResponseDto>
{
    [FromBody]
    public TelegramSessionDto Model { get; set; }
}

public class CheckConsoleSessionResponseDto
{
    public bool IsSessionValid { get; set; }
    public string BearerToken { get; set; }
}

public class CheckConsoleSessionHandler(
    ILogger<CheckConsoleSessionHandler> logger,
    DataFacade dataFacade,
    AppSettingRepository appSettingRepository
)
    : IWebRequestHandler<CheckConsoleSessionRequest, CheckConsoleSessionResponseDto>
{
    public async Task<WebResponseBase<CheckConsoleSessionResponseDto>> Handle(
        CheckConsoleSessionRequest request,
        CancellationToken cancellationToken
    )
    {
        WebResponseBase<CheckConsoleSessionResponseDto> response = new();

        var botSetting = await dataFacade.MongoDb.BotSettings
            .Where(entity => entity.Status == EventStatus.Complete)
            .Where(entity => entity.Name == "Main")
            .FirstOrDefaultAsync(cancellationToken);

        if (botSetting == null)
        {
            return response.BadRequest("Suatu kesalahan terjadi, silahkan hubungi admin");
        }

        LoginWidget loginWidget = new(botSetting.Token);

        var sessionData = request.Model.ToDictionary()
            .Select(pair => new KeyValuePair<string,string>(pair.Key.Replace(" ","_"),pair.Value.ToString() ?? string.Empty))
            .Where(pair => pair.Value.IsNotNullOrEmpty())
            .Where(pair => pair.Key != "session_id");

        var checkAuthorization = loginWidget.CheckAuthorization(sessionData);
        if (checkAuthorization != WebAuthorization.Valid)
        {
            logger.LogDebug("Session is not valid for SessionId: {SessionId}. Result: {Result}",
                request.Model.SessionId, checkAuthorization);

            return response.Unauthorized(
                $"Sesi tidak valid, silakan kirim ulang perintah '/console' di Bot untuk membuat sesi baru.");
        }

        var jwtConfig = await appSettingRepository.GetConfigSectionAsync<JwtConfig>();

        if (jwtConfig == null)
        {
            return response.BadRequest("Authentication is not yet configured");
        }

        var roles = new List<string>() {
            "Guest"
        };

        var findSudo = await dataFacade.MongoDb.Sudoers.AsNoTracking()
            .Where(x => x.Status == EventStatus.Complete)
            .Where(x => x.UserId == request.Model.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (findSudo != null)
        {
            roles.Add("Sudo");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>() {
            new(ClaimTypes.NameIdentifier, request.Model.Username ?? ""),
            new(ClaimTypes.Name, request.Model.FirstName ?? ""),
            new(ClaimTypes.Surname, request.Model.LastName ?? ""),
            new(RequestKey.UserId, request.Model.Id.ToString()),
            new("photoUrl", request.Model.PhotoUrl ?? "")
        };

        roles.ForEach(role => {
            claims.Add(new(ClaimTypes.Role, role));
        });

        var token = new JwtSecurityToken(jwtConfig.Issuer, jwtConfig.Audience, claims,
            expires: DateTime.Now.AddMinutes(15), signingCredentials: credentials);

        var stringToken = new JwtSecurityTokenHandler().WriteToken(token);

        dataFacade.MongoDb.DashboardSessions.Add(new() {
            TelegramUserId = request.Model.Id,
            FirstName = request.Model.FirstName,
            LastName = request.Model.LastName,
            PhotoUrl = request.Model.PhotoUrl,
            Username = request.Model.Username,
            AuthDate = request.Model.AuthDate,
            Hash = request.Model.Hash,
            SessionId = request.Model.SessionId,
            BearerToken = stringToken,
            Status = EventStatus.Complete
        });

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return response.Success("Session saved successfully", new() {
            IsSessionValid = true,
            BearerToken = stringToken
        });
    }
}