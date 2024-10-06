using Allowed.Telegram.Bot.Models;
using Microsoft.Extensions.Options;
using Xunit;

namespace ZiziBot.Tests.Pipelines;

public class PingTest(MediatorService mediatorService, IOptionsSnapshot<List<SimpleTelegramBotClientOptions>> botOptions)
{
    private List<SimpleTelegramBotClientOptions> ListBotData => botOptions.Value;

    [Fact]
    public async Task Ping()
    {
        foreach (var botData in ListBotData)
        {
            await mediatorService.EnqueueAsync(new PingBotRequestModel
            {
                BotToken = botData.Token,
                Message = SampleMessages.CommonMessage
            });
        }

        Assert.True(true);
    }
}