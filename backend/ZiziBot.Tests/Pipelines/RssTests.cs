using Microsoft.EntityFrameworkCore;
using Xunit;
using ZiziBot.Application.Features.UseCases.Rss;
using ZiziBot.Application.Common.Enums;
using ZiziBot.Application.Common.Utils;
using ZiziBot.Application.Infrastructure.Database.Service;

namespace ZiziBot.Tests.Pipelines;

public class RssTests(
    MediatorService mediatorService,
    DataFacade dataFacade,
    FetchRssUseCase fetchRssUseCase
)
{
    [Theory]
    [InlineData("https://devblogs.microsoft.com/dotnet/feed/")]
    [InlineData("https://github.com/j-hc/revanced-magisk-module/releases.atom")]
    [InlineData("https://github.com/revanced-apks/build-apps/releases.atom")]
    public async Task FetchTest(string url)
    {
        var historyEntity = await dataFacade.MongoDb.RssHistory
            .Where(entity => entity.RssUrl == url)
            .ToListAsync();

        historyEntity.ForEach(x => {
            x.Status = (int)EventStatus.Deleted;
        });

        await dataFacade.MongoDb.SaveChangesAsync();

        await fetchRssUseCase.Handle(
            chatId: 0,
            threadId: 0,
            rssUrl: url
        );
    }


    [Fact]
    public async Task FetchFromDbTest()
    {
        var listRss = await dataFacade.MongoDb.RssSetting
            .OrderByDescending(x => x.UpdatedDate)
            .ToListAsync();

        var ran = listRss
            .GroupBy(x => x.ChatId)
            .Select(x => x.RandomPick())
            .DistinctBy(x => x.RssUrl);

        foreach (var rss in ran)
        {
            var historyEntity = await dataFacade.MongoDb.RssHistory
                .Where(entity => entity.RssUrl == rss.RssUrl)
                .ToListAsync();

            historyEntity.ForEach(x =>
            {
                x.Status = (int)EventStatus.Deleted;
            });

            await dataFacade.MongoDb.SaveChangesAsync();

            var result = await fetchRssUseCase.Handle(
                chatId: rss.ChatId,
                threadId: rss.ThreadId,
                rssUrl: rss.RssUrl
            );

            result.ShouldBeTrue();
        }
    }
}
