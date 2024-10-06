using MongoFramework.Linq;
using Xunit;

namespace ZiziBot.Tests.Pipelines;

public class RssTests(MediatorService mediatorService, MongoDbContextBase mongoDbContextBase)
{
    [Theory]
    [InlineData("https://devblogs.microsoft.com/dotnet/feed/")]
    [InlineData("https://github.com/j-hc/revanced-magisk-module/releases.atom")]
    [InlineData("https://github.com/revanced-apks/build-apps/releases.atom")]
    public async Task FetchTest(string url)
    {
        var historyEntity = await mongoDbContextBase.RssHistory
            .Where(entity => entity.RssUrl == url)
            .ToListAsync();

        historyEntity.ForEach(x => {
            x.Status = (int)EventStatus.Deleted;
        });

        await mongoDbContextBase.SaveChangesAsync();


        await mediatorService.Send(new FetchRssRequest()
        {
            ChatId = -1001710313973,
            RssUrl = url
        });
    }
}