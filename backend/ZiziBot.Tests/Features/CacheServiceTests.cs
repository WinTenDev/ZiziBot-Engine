using Xunit;
using ZiziBot.Application.Common.Dtos;
using ZiziBot.Application.Common.Types;
using ZiziBot.Application.Infrastructure.Database.Service;

namespace ZiziBot.Tests.Features;

public class CacheServiceTests(CacheService cacheService)
{
    [Fact]
    public async Task WriteToCache()
    {
        var testData1 = "testData1";

        var cacheData1 = await cacheService.GetOrSetAsync(cacheKey: "test", action: () => Task.FromResult(testData1));
        testData1.ShouldBeEquivalentTo(cacheData1);

        var guidData = Guid.NewGuid();
        var cacheGuid = await cacheService.GetOrSetAsync(cacheKey: "test-guid", action: () => Task.FromResult(guidData));
        guidData.ShouldBeEquivalentTo(cacheGuid);
    }

    [Fact]
    public async Task GetSetV2()
    {
        await cacheService.GetOrSetAsync(
            cacheKey: "a",
            action: async () =>
            {
                await Task.Delay(0);
                return new AntiSpamDto();
            }
        );
    }
}
