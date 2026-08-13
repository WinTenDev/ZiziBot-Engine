using Microsoft.EntityFrameworkCore;
using Xunit;
using ZiziBot.Application.Common.Enums;
using ZiziBot.Application.Common.Utils;
using ZiziBot.Application.Infrastructure.Database.MongoDb;

namespace ZiziBot.Tests.Pipelines;

public class CityTest(
    MediatorService mediatorService,
    BotRepository botRepository,
    MongoDbContext mongoDbContext,
    FathimahRestService fathimahRestService
)
{
    [Theory]
    [InlineData(712)]
    public async Task AddCityTest(int cityId)
    {
        var botMain = await botRepository.GetBotMain();

        // Arrange
        var botResponseBase = await mediatorService.Send(new AddCityBotRequest()
        {
            BotToken = botMain.Token,
            Message = SampleMessages.CommonMessage,
            ReplyMessage = true,
            CityId = cityId
        });


        botResponseBase.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("Cilacap")]
    public async Task AddCityByNameTest(string cityName)
    {
        var chatId = SampleMessages.CommonMessage.Chat.Id;
        var botMain = await botRepository.GetBotMain();
        var cityInfoAll = await fathimahRestService.GetAllCityAsync();

        var cityInfo = cityInfoAll.Cities
            .WhereIf(cityName.IsNotNullOrEmpty(), kota => kota.Lokasi.Contains(cityName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        var city = await mongoDbContext.BangHasan_ShalatCity
            .Where(entity => entity.ChatId == chatId)
            .Where(entity => entity.CityId == cityInfo.Id)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync();

        if (city != null)
        {
            city.Status = (int)EventStatus.Deleted;
            await mongoDbContext.SaveChangesAsync();
        }

        // Arrange
        var botResponse = await mediatorService.Send(new AddCityBotRequest()
        {
            BotToken = botMain.Token,
            Message = SampleMessages.CommonMessage,
            ReplyMessage = true,
            CityName = cityName
        });

        botResponse.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("Cilacap")]
    public async Task AddCityByNameAlreadyAddedTest(string cityName)
    {
        var botMain = await botRepository.GetBotMain();

        // Arrange
        var botResponse = await mediatorService.Send(new AddCityBotRequest()
        {
            BotToken = botMain.Token,
            Message = SampleMessages.CommonMessage,
            ReplyMessage = true,
            CityName = cityName
        });

        botResponse.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(-1001404591750)]
    public async Task ShalatTimeTest(long chatId)
    {
        var response = await mediatorService.Send(new SendShalatTimeRequest()
        {
            ChatId = chatId
        });

        response.ShouldBeTrue();
    }
}
