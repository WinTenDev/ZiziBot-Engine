using Microsoft.Extensions.Configuration;
using Xunit;
using ZiziBot.Application.Infrastructure.Database.Service;

namespace ZiziBot.Tests.Pipelines;

public class MemberChangesTest(MediatorService mediatorService, IConfiguration configuration, DataFacade dataFacade)
{
    [Fact]
    public async Task NewChatMembers()
    {
        var bots = await dataFacade.Bot.ListBots();

        foreach (var botData in bots)
        {
            await mediatorService.EnqueueAsync(new NewChatMembersBotRequest() {
                BotToken = botData.Token,
                Message = SampleMessages.NewChatMembers,
                NewUser = SampleMessages.NewChatMembers.NewChatMembers!
            });
        }

        true.ShouldBeTrue();
    }
}
