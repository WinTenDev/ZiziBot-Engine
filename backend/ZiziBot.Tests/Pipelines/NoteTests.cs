using Xunit;
using ZiziBot.Application.Core;
using ZiziBot.Application.Common.Enums;
using ZiziBot.Application.Infrastructure.Database.Service;

namespace ZiziBot.Tests.Pipelines;

public class NoteTests(MediatorService mediatorService, DataFacade dataFacade)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNoteTest(bool refreshNote)
    {
        var botMain = await dataFacade.Bot.GetBotMain();

        var result = await mediatorService.EnqueueAsync(new CreateNoteBotRequest() {
            BotToken = botMain.Token,
            Message = SampleMessages.CommonMessage,
            Query = "sample_query",
            Content = "lorem ipsum dolor",
            RawButton = "example|https://azhe.my.id",
            FileId = "AgACAgUAAx0CU7hehgABBIhxZFYPJ554nuA85DnmWX9_B33eTx8AAv-yMRuEgPFV7bW8s4uaTKABAAMCAAN5AAMvBA",
            DataType = (int)CommonMediaType.Photo,
            RefreshNote = refreshNote
        });

        result.ResponseSource.ShouldBe(ResponseSource.Bot);
    }

    [Theory]
    [InlineData("ini-note-ngab")]
    public async Task DeleteNoteTest(string note)
    {
        // Arrange
        dataFacade.MongoDb.Note.Add(new() {
            ChatId = SampleMessages.CommonMessage.Chat.Id,
            Query = note,
            Status = EventStatus.Complete
        });

        await dataFacade.MongoDb.SaveChangesAsync();


        // Act
        var botMain = await dataFacade.Bot.GetBotMain();

        var response = await mediatorService.EnqueueAsync(new DeleteNoteRequest() {
            BotToken = botMain.Token,
            Message = SampleMessages.CommonMessage,
            Note = note
        });

        response.ShouldBeOfType<BotResponseBase>();
    }

    [Theory]
    [InlineData("ini-note-ngab")]
    public async Task DeleteNoteAlreadyDeletedTest(string note)
    {
        var botMain = await dataFacade.Bot.GetBotMain();

        var response = await mediatorService.EnqueueAsync(new DeleteNoteRequest() {
            BotToken = botMain.Token,
            Message = SampleMessages.CommonMessage,
            Note = note
        });

        response.ShouldBeOfType<BotResponseBase>();
    }
}
