﻿using Telegram.Bot.Types;

namespace ZiziBot.Contracts.Dtos;

public class ChatActivityDto
{
    public required long ChatId { get; set; }
    public required ChatActivityType ActivityType { get; set; }
    public required Chat Chat { get; set; }
    public required User User { get; set; }
}