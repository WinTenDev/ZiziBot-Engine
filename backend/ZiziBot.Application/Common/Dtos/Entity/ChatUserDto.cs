using Telegram.Bot.Types.Enums;

namespace ZiziBot.Application.Common.Dtos.Entity;

public class ChatUserDto
{
    public long ChatId { get; set; }
    public ChatType ChatType { get; set; }
    public string? ChatTitle { get; set; }
    public string? ChatUsername { get; set; }
    public int MemberCount { get; set; }
    public bool IsBotAdmin { get; set; }
    public string TransactionId { get; set; }
}