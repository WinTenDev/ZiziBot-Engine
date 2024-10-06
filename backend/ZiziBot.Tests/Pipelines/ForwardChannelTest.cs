using Xunit;

namespace ZiziBot.Tests.Pipelines;

public class ForwardChannelTest(MediatorService mediatorService, AppSettingRepository appSettingRepository)
{
    [Fact]
    public async Task SendChannelPostTest()
    {
        var botMain = await appSettingRepository.GetBotMain();

        await mediatorService.Send(new ForwardChannelPostRequest()
        {
            BotToken = botMain.Token,
            Update = SampleMessages.UpdateChannelPost
        });
    }

    [Fact]
    public async Task SendChannelPostDocumentTest()
    {
        var botMain = await appSettingRepository.GetBotMain();

        await mediatorService.Send(new ForwardChannelPostRequest()
        {
            BotToken = botMain.Token,
            Update = SampleMessages.UpdateChannelPostDocument
        });
    }
}