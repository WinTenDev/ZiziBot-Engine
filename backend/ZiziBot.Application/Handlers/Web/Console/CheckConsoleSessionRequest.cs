using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoFramework.Linq;

namespace ZiziBot.Application.Handlers.Web.Console;

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

public class CheckConsoleSessionHandler : IRequestHandler<CheckConsoleSessionRequest, WebResponseBase<CheckConsoleSessionResponseDto>>
{
    private readonly ILogger<CheckConsoleSessionHandler> _logger;
    private readonly MongoDbContextBase _mongoDbContext;
    private readonly AppSettingRepository _appSettingRepository;

    public CheckConsoleSessionHandler(
        ILogger<CheckConsoleSessionHandler> logger,
        MongoDbContextBase mongoDbContext,
        AppSettingRepository appSettingRepository
    )
    {
        _logger = logger;
        _mongoDbContext = mongoDbContext;
        _appSettingRepository = appSettingRepository;
    }

    public async Task<WebResponseBase<CheckConsoleSessionResponseDto>> Handle(CheckConsoleSessionRequest request, CancellationToken cancellationToken)
    {
        WebResponseBase<CheckConsoleSessionResponseDto> response = new();

        var botSetting = await _mongoDbContext.BotSettings
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .Where(entity => entity.Name == "Main")
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (botSetting == null)
        {
            return response.BadRequest("Suatu kesalahan terjadi, silahkan hubungi admin");
        }

        LoginWidget loginWidget = new(botSetting.Token);

        var sessionData = request.Model.ToDictionary()
            .Select(pair => new KeyValuePair<string, string>(pair.Key.Replace(" ", "_"), pair.Value))
            .Where(pair => pair.Value.IsNotNullOrEmpty())
            .Where(pair => pair.Key != "session_id");

        var checkAuthorization = loginWidget.CheckAuthorization(sessionData);
        if (checkAuthorization != WebAuthorization.Valid)
        {
            _logger.LogDebug("Session is not valid for SessionId: {SessionId}. Result: {Result}", request.Model.SessionId, checkAuthorization);
            return response.Unauthorized($"Sesi tidak valid, silakan kirim ulang perintah '/console' di Bot untuk membuat sesi baru.");
        }

        var jwtConfig = await _appSettingRepository.GetConfigSectionAsync<JwtConfig>();

        if (jwtConfig == null)
        {
            return response.BadRequest("Authentication is not yet configured");
        }

        var roles = new List<string>()
        {
            "Guest"
        };

        var findSudo = await _mongoDbContext.Sudoers.AsNoTracking()
                                            .Where(x => x.Status == (int)EventStatus.Complete)
                                            .Where(x => x.UserId == request.Model.Id)
                                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (findSudo != null)
        {
            roles.Add("Sudo");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, request.Model.Username),
            new Claim(ClaimTypes.Name, request.Model.FirstName),
            new Claim(HeaderKey.UserId, request.Model.Id.ToString()),
            new Claim("photoUrl", request.Model.PhotoUrl ?? ""),
        };

        roles.ForEach(role => {
            claims.Add(new Claim(ClaimTypes.Role, role));
        });

        var token = new JwtSecurityToken(jwtConfig.Issuer, jwtConfig.Audience, claims, expires: DateTime.Now.AddMinutes(15), signingCredentials: credentials);
        var stringToken = new JwtSecurityTokenHandler().WriteToken(token);

        _mongoDbContext.DashboardSessions.Add(new DashboardSessionEntity()
        {
            TelegramUserId = request.Model.Id,
            FirstName = request.Model.FirstName,
            LastName = request.Model.LastName,
            PhotoUrl = request.Model.PhotoUrl,
            Username = request.Model.Username,
            AuthDate = request.Model.AuthDate,
            Hash = request.Model.Hash,
            SessionId = request.Model.SessionId,
            BearerToken = stringToken,
            Status = (int)EventStatus.Complete
        });

        await _mongoDbContext.SaveChangesAsync(cancellationToken);

        return response.Success("Session saved successfully", new CheckConsoleSessionResponseDto()
        {
            IsSessionValid = true,
            BearerToken = stringToken
        });
    }
}