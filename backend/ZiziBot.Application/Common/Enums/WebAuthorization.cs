namespace ZiziBot.Application.Common.Enums;

//  Original source: https://github.com/TelegramBots/Telegram.Bot.Extensions.LoginWidget/blob/master/src/Telegram.Bot.Extensions.LoginWidget/Authorization.cs
public enum WebAuthorization
{
    InvalidHash,
    MissingFields,
    InvalidAuthDateFormat,
    TooOld,
    Valid
}