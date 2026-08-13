using Xunit;
using ZiziBot.Application.Infrastructure.Config;

namespace ZiziBot.Tests.Repository;

public class AppSettingTest(AppSettingRepository appSettingRepository)
{
    [Fact]
    public async Task GetAppSetting()
    {
        var config = await appSettingRepository.GetConfigSectionAsync<EngineConfig>();

        config.ShouldNotBeNull();
    }
}