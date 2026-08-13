namespace ZiziBot.Application.Common.Enums;

public enum ChatActivityType
{
    BotBanUser,
    BotUnbanUser,
    BotKickMember,

    BotEditMessage,
    BotDeleteMessage,
    BotSendMessage,
    BotWarnUsername,

    BotSendWebHook,
    BotEditWebHook,

    NewChatMember,
    KickMember,

    UserDeleteMessage,
    UserForwardMessage,
    UserPinMessage,
    UserUnpinMessage,

    UserSendMessage,
    UserEditMessage,
}