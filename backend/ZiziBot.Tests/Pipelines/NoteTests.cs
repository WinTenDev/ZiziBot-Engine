using FluentAssertions;
using Xunit;

namespace ZiziBot.Tests.Pipelines;

public class NoteTests(MediatorService mediatorService, AppSettingRepository appSettingRepository, MongoDbContextBase mongoDbContext)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNoteTest(bool refreshNote)
    {
        var botMain = await appSettingRepository.GetBotMain();

        var result = await mediatorService.EnqueueAsync(new CreateNoteBotRequest()
        {
            BotToken = botMain.Token,
            Message = SampleMessages.CommonMessage,
            Query = "sample_query",
            Content = "lorem ipsum dolor",
            RawButton = "example|https://azhe.my.id",
            FileId = "AgACAgUAAx0CU7hehgABBIhxZFYPJ554nuA85DnmWX9_B33eTx8AAv-yMRuEgPFV7bW8s4uaTKABAAMCAAN5AAMvBA",
            DataType = (int)CommonMediaType.Photo,
            RefreshNote = refreshNote
        });

        result.ResponseSource.Should().Be(ResponseSource.Bot);
    }

    [Theory]
    [InlineData("ini-note-ngab")]
    public async Task DeleteNoteTest(string note)
    {
        // Arrange
        mongoDbContext.Note.Add(new NoteEntity()
        {
            ChatId = SampleMessages.CommonMessage.Chat.Id,
            Query = note,
            Status = (int)EventStatus.Complete
        });

        await mongoDbContext.SaveChangesAsync();


        // Act
        var botMain = await appSettingRepository.GetBotMain();

        await mediatorService.EnqueueAsync(new DeleteNoteRequest()
        {
            BotToken = botMain.Token,
            Message = SampleMessages.CommonMessage,
            Note = note
        });
    }

    [Theory]
    [InlineData("ini-note-ngab")]
    public async Task DeleteNoteAlreadyDeletedTest(string note)
    {
        var botMain = await appSettingRepository.GetBotMain();

        await mediatorService.EnqueueAsync(new DeleteNoteRequest()
        {
            BotToken = botMain.Token,
            Message = SampleMessages.CommonMessage,
            // ReplyMessage = true,
            Note = note
        });
    }
}