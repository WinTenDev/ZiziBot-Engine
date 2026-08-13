using Microsoft.EntityFrameworkCore;
using Xunit;
using ZiziBot.Application.Common.Enums;
using ZiziBot.Application.Common.Utils;
using ZiziBot.Application.Infrastructure.Database.Service;

namespace ZiziBot.Tests.Pipelines;

public class MirrorUserTest(
    MediatorService mediatorService,
    DataFacade dataFacade
)
{
    [Theory]
    [InlineData("ca9c28da-87c0-5d32-9b6f-a220d3d36dfd")] // trakteer
    [InlineData("https://trakteer.id/payment-status/94537cf1-b8a3-5c57-acfd-dd3705476d68")] // trakteerUrl
    [InlineData("65190576-e653-47d2-b472-9a367a54ed23")] //saweria
    public async Task SubmitTrakteerPaymentTest(string url)
    {
        // Arrange
        var orderId = url.UrlSegment(1, url);
        var bot = await dataFacade.Bot.GetBotMain();
        var payment = await dataFacade.MongoDb.MirrorApproval
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(entity => entity.OrderId == orderId);

        if (payment != null)
        {
            payment.Status = (int)EventStatus.Deleted;

            await dataFacade.MongoDb.SaveChangesAsync();
        }

        bot.ShouldNotBeNull();

        await mediatorService.Send(new SubmitPaymentBotRequest() {
            BotToken = bot.Token,
            Message = SampleMessages.CommonMessage,
            Payload = orderId
        });
    }

    [Theory]
    [InlineData("ca9c28da-87c0-5d32-9b6f-a220d3d36dfd", 12345)] // trakteer
    public async Task SubmitTrakteerPaymentForUserIdTest(string url, long userId)
    {
        // Arrange
        var bot = await dataFacade.Bot.GetBotMain();
        var payment = await dataFacade.MongoDb.MirrorApproval
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(entity => entity.OrderId == url);

        if (payment != null)
        {
            payment.Status = EventStatus.Deleted;

            await dataFacade.MongoDb.SaveChangesAsync();
        }

        // Assert
        bot.ShouldNotBeNull();

        await mediatorService.Send(new SubmitPaymentBotRequest() {
            BotToken = bot.Token,
            Message = SampleMessages.CommonMessage,
            Payload = url,
            ForUserId = userId
        });
    }

    [Theory]
    [InlineData("ca9c28da-87c0-5d32-9b6f-a220d3d36dfd")] // trakteer
    public async Task SubmitTrakteerPaymentConfirmationExpiredTest(string url)
    {
        // Arrange
        var bot = await dataFacade.Bot.GetBotMain();
        await dataFacade.AppSetting.UpdateAppSetting("Mirror:PaymentExpirationDays", "3");

        bot.ShouldNotBeNull();

        await mediatorService.Send(new SubmitPaymentBotRequest() {
            BotToken = bot.Token,
            Message = SampleMessages.CommonMessage,
            Payload = url
        });

        await dataFacade.AppSetting.UpdateAppSetting("Mirror:PaymentExpirationDays", "300");
    }

    [Theory]
    [InlineData("ca9c28da-87c0-5d32-9b6f-a220d3d36dfd")] // trakteer
    public async Task SubmitTrakteerPaymentAlreadyPaidTest(string url)
    {
        // Arrange
        var bot = await dataFacade.Bot.GetBotMain();

        bot.ShouldNotBeNull();

        await mediatorService.Send(new SubmitPaymentBotRequest() {
            BotToken = bot.Token,
            Message = SampleMessages.CommonMessage,
            Payload = url
        });
    }

    [Theory]
    [InlineData("dummy-order-id")]
    public async Task SubmitTrakteerPaymentInvalidOrderIdTest(string url)
    {
        // Arrange
        var bot = await dataFacade.Bot.GetBotMain();

        bot.ShouldNotBeNull();

        var response = await mediatorService.Send(new SubmitPaymentBotRequest() {
            BotToken = bot.Token,
            Message = SampleMessages.CommonMessage,
            Payload = url
        });

        response.ShouldBeNull();
    }
}
