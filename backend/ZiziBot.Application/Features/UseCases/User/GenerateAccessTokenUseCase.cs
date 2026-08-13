using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ZiziBot.Application.Common.Exceptions;
using ZiziBot.Application.Infrastructure.Database.MongoDb;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.UseCases.User;

public class GenerateAccessTokenResult
{
    public required string AccessToken { get; set; }
    public double AccessExpireIn { get; set; }
    public DateTime AccessExpireDate { get; set; }
}

public class GenerateAccessTokenUseCase(
    MongoDbContext mongoDbContext,
    AppSettingRepository appSettingRepository
)
{
    public async Task<GenerateAccessTokenResult> Handle(UserOtpEntity userOtp)
    {
        var (userId, tokenExpiration, accessExpireIn, accessToken) = await GetAccessToken(userOtp);

        var botUser = await mongoDbContext.BotUser.AsNoTracking()
            .Where(x => x.Status == EventStatus.Complete)
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();

        mongoDbContext.DashboardSessions.Add(new DashboardSessionEntity
        {
            Status = EventStatus.Complete,
            TransactionId = userOtp.TransactionId,
            CreatedBy = userId,
            UpdatedBy = userId,
            TelegramUserId = userId,
            FirstName = botUser?.FirstName,
            LastName = botUser?.LastName,
            Username = botUser?.Username,
            PhotoUrl = botUser?.ProfilePhotoId,
            SessionId = userOtp.TransactionId,
            BearerToken = accessToken,
            ExpireDate = tokenExpiration
        });

        await mongoDbContext.SaveChangesAsync();

        return new GenerateAccessTokenResult
        {
            AccessToken = accessToken,
            AccessExpireIn = accessExpireIn,
            AccessExpireDate = tokenExpiration
        };
    }

    private async Task<(long userId, DateTime tokenExpiration, double accessExpireIn, string accessToken)> GetAccessToken(UserOtpEntity userOtp)
    {
        var jwtConfig = await appSettingRepository.GetConfigSectionAsync<JwtConfig>();

        if (jwtConfig == null)
            throw new AppException("JWT Config not found");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var userId = userOtp.UserId;

        var claims = new[] { new Claim(RequestKey.UserId, userId.ToString()), };

        var dateTime = DateTime.UtcNow;
        var tokenExpiration = dateTime.AddMinutes(15);
        var accessExpireIn = (tokenExpiration - dateTime).TotalSeconds;
        var token = new JwtSecurityToken(issuer: jwtConfig.Issuer,
            audience: jwtConfig.Audience,
            claims: claims,
            expires: tokenExpiration,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return (userId, tokenExpiration, accessExpireIn, accessToken);
    }
}
