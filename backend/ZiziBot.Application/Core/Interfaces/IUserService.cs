using JetBrains.Annotations;

namespace ZiziBot.Application.Core.Interfaces;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IUserService
{
    Task<(string stringToken, DateTime tokenExpiration, int accessExpireIn)> GenerateAccessToken(long userId);
}