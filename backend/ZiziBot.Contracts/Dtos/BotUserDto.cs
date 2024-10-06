using Telegram.Bot.Types;

namespace ZiziBot.Contracts.Dtos;

public class BotUserDto
{
    public User User { get; set; }
    public long UserId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? LanguageCode { get; set; }
    public string TransactionId { get; set; }
}