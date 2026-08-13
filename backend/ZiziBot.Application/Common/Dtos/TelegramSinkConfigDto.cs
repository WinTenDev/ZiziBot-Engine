namespace ZiziBot.Application.Common.Dtos;

public class TelegramSinkConfigDto
{
    public string? BotToken { get; init; }
    public long? ChatId { get; init; }
    public int? ThreadId { get; set; }
}