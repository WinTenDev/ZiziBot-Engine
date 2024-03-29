using System.Diagnostics.CodeAnalysis;

namespace ZiziBot.Contracts.Constants;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class CacheKey
{
    public const string BAN_ESS = "ban/ess/";
    public const string BAN_CAS = "ban/cas/";
    public const string BAN_SW = "ban/sw/";

    public const string SUDO = "sudo/";
    public const string LIST_CHAT_ADMIN = "chat/admin/";

    public const string ACTIVE_USERNAMES_CHAT = "active-usernames/chat/";
    public const string ACTIVE_USERNAMES_USER = "active-usernames/user/";

    public const string API_DOC = "api-doc";

    public const string BADWORD = "badword";
}