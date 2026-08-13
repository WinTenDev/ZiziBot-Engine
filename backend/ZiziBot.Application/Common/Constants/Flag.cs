using System.ComponentModel;
using System.Reflection;
using ZiziBot.Application.Common.Dtos;

// ReSharper disable InconsistentNaming
namespace ZiziBot.Application.Common.Constants;

public static class Flag
{
    public static List<FlagDto> GetFields()
    {
        var properties = typeof(Flag).GetFields();

        var values = properties.Select(x => new FlagDto() {
            Name = x.Name,
            Value = (bool)(x.GetCustomAttribute<DefaultValueAttribute>()?.Value ?? false)
        }).ToList();

        return values;
    }

    #region Flags
    #region Infrastructure
    [DefaultValue(true)]
    public const string CONSOLE_BLAZOR = "CONSOLE_BLAZOR";

    [DefaultValue(true)]
    public const string HANGFIRE = "HANGFIRE";

    [DefaultValue(true)]
    public const string HANGFIRE_SEPARATED_SERVER = "HANGFIRE_SEPARATED_SERVER";

    [DefaultValue(false)]
    public const string HANGFIRE_ENABLE_AUTH = "HANGFIRE_ENABLE_AUTH";

    [DefaultValue(true)]
    public const string RSS_BROADCASTER = "RSS_BROADCASTER";

    [DefaultValue(false)]
    public const string SALAT_TIME = "SALAT_TIME";

    [DefaultValue(false)]
    public const string ANTI_RAID = "ANTI_RAID";

    [DefaultValue(false)]
    public const string RSS_INCLUDE_CONTENT = "RSS_INCLUDE_CONTENT";

    [DefaultValue(true)]
    public const string RSS_RESET_AT_STARTUP = "RSS_RESET_AT_STARTUP";
    #endregion

    #region Bot Commands
    [DefaultValue(true)]
    public const string COMMAND_PING = "COMMAND_PING";

    [DefaultValue(true)]
    public const string COMMAND_SP = "COMMAND_SP";

    [DefaultValue(true)]
    public const string COMMAND_MP = "COMMAND_MP";
    #endregion

    #region Rest API
    [DefaultValue(true)]
    public const string REST_USER_INFO_GET = "REST_USER_INFO_GET";

    [DefaultValue(true)]
    public const string REST_USER_TELEGRAM_SESSION_CREATE = "REST_USER_TELEGRAM_SESSION_CREATE";

    [DefaultValue(true)]
    public const string REST_USER_SESSION_OTP_POST = "REST_USER_SESSION_OTP_POST";

    [DefaultValue(true)]
    public const string REST_USER_GROUP_LIST = "REST_USER_GROUP_LIST";

    [DefaultValue(true)]
    public const string REST_CHAT_NOTE_CREATE = "REST_CHAT_NOTE_CREATE";

    [DefaultValue(true)]
    public const string REST_CHAT_NOTE_LIST = "REST_CHAT_NOTE_LIST";

    [DefaultValue(true)]
    public const string REST_CHAT_NOTE_GET = "REST_CHAT_NOTE_GET";

    [DefaultValue(true)]
    public const string REST_CHAT_RSS_LIST = "REST_CHAT_RSS_LIST";

    [DefaultValue(true)]
    public const string REST_CHAT_RSS_LIST_HISTORY = "REST_CHAT_RSS_LIST_HISTORY";

    [DefaultValue(true)]
    public const string REST_CHAT_RSS_CREATE = "REST_CHAT_RSS_CREATE";

    [DefaultValue(true)]
    public const string REST_CHAT_NOTE_DELETE = "REST_CHAT_NOTE_DELETE";

    [DefaultValue(true)]
    public const string REST_ANTISPAM_ESS_CREATE = "REST_ANTISPAM_ESS_CREATE";

    [DefaultValue(true)]
    public const string REST_ANTISPAM_ESS_DELETE = "REST_ANTISPAM_ESS_DELETE";

    [DefaultValue(true)]
    public const string REST_ANTISPAM_ESS_GET_LIST = "";

    [DefaultValue(true)]
    public const string REST_ANTISPAM_ESS_UNDELETE = "";

    [DefaultValue(true)]
    public const string REST_ANTISPAM_ESS_CHECK_BAN = "";

    [DefaultValue(true)]
    public const string REST_GROUP_WELCOME_MESSAGE_CREATE = "REST_GROUP_WELCOME_MESSAGE_CREATE";

    [DefaultValue(true)]
    public const string REST_GROUP_WELCOME_MESSAGE_UPDATE = "REST_GROUP_WELCOME_MESSAGE_UPDATE";

    [DefaultValue(true)]
    public const string REST_GROUP_WELCOME_MESSAGE_SELECT = "REST_GROUP_WELCOME_MESSAGE_SELECT";

    [DefaultValue(true)]
    public const string REST_GROUP_WELCOME_MESSAGE_GET_LIST = "REST_GROUP_WELCOME_MESSAGE_GET_LIST";

    [DefaultValue(true)]
    public const string REST_GROUP_WELCOME_MESSAGE_GET_DETAIL = "REST_GROUP_WELCOME_MESSAGE_GET_DETAIL";

    [DefaultValue(true)]
    public const string REST_GROUP_WELCOME_MESSAGE_DELETE = "REST_GROUP_WELCOME_MESSAGE_DELETE";

    [DefaultValue(true)]
    public const string REST_MIRROR_USER_CREATE = "REST_MIRROR_USER_CREATE";

    [DefaultValue(true)]
    public const string REST_MIRROR_SUBMIT_DONATION = "REST_MIRROR_SUBMIT_DONATION";

    [DefaultValue(true)]
    public const string REST_MIRROR_USER_CHECK_USER = "REST_MIRROR_USER_CHECK_USER";

    [DefaultValue(true)]
    public const string REST_MIRROR_USER_CHECK_ORDER = "REST_MIRROR_USER_CHECK_ORDER";

    [DefaultValue(true)]
    public const string REST_MIRROR_USER_GET_LIST = "REST_MIRROR_USER_GET_LIST";

    [DefaultValue(true)]
    public const string REST_MIRROR_USER_DELETE = "REST_MIRROR_USER_DELETE";

    [DefaultValue(true)]
    public const string REST_MIRROR_TRAKTEER_WEBHOOK = "REST_MIRROR_TRAKTEER_WEBHOOK";

    [DefaultValue(true)]
    public const string REST_MIRROR_VERIFY_USER = "REST_MIRROR_VERIFY_USER";

    [DefaultValue(true)]
    public const string REST_PRODUCTIVITY_PENDEKIN_CREATE = "REST_PRODUCTIVITY_PENDEKIN_CREATE";

    [DefaultValue(true)]
    public const string REST_PRODUCTIVITY_PENDEKIN_LIST = "REST_PRODUCTIVITY_PENDEKIN_LIST";

    [DefaultValue(true)]
    public const string REST_PRODUCTIVITY_PENDEKIN_GET = "REST_PRODUCTIVITY_PENDEKIN_GET";

    [DefaultValue(true)]
    public const string REST_PRODUCTIVITY_PENDEKIN_DELETE = "REST_PRODUCTIVITY_PENDEKIN_DELETE";

    [DefaultValue(true)]
    public const string REST_PRODUCTIVITY_WEBHOOK = "REST_PRODUCTIVITY_WEBHOOK";

    [DefaultValue(true)]
    public const string REST_API_KEY_GET = "REST_API_KEY_GET";

    [DefaultValue(true)]
    public const string REST_APP_SETTING_GET_LIST = "REST_APP_SETTING_GET_LIST";

    #endregion
    #endregion
}