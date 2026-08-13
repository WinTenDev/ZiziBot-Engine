using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.Application.Common.Dtos;

public class ChatAdminDto
{
    public User User { get; set; }
    public ChatMemberStatus Status { get; set; }
    public string? CustomTitle { get; set; }
    public bool IsAnonymous { get; set; }
}