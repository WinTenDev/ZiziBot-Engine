using Allowed.Telegram.Bot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace ZiziBot.Tests.Pipelines;

public class MemberChangesTest(MediatorService mediatorService, IOptionsSnapshot<List<SimpleTelegramBotClientOptions>> botOptions, IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    private List<SimpleTelegramBotClientOptions> ListBotData => botOptions.Value;

    [Fact]
    public async Task NewChatMembers()
    {
        foreach (var botData in ListBotData)
        {
            await mediatorService.EnqueueAsync(new NewChatMembersBotRequest()
            {
                BotToken = botData.Token,
                Message = SampleMessages.NewChatMembers,
                NewUser = SampleMessages.NewChatMembers.NewChatMembers!
            });
        }

        Assert.True(true);
    }
}