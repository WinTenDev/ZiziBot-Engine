using ZiziBot.Application.Common.Enums;

namespace ZiziBot.Application.Core.Interfaces;

public interface IApiKeyService
{
    public Task<string> GetApiKeyAsync(ApiKeyCategory category, ApiKeyVendor name);
}