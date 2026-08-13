using ZiziBot.Application.Common.Enums;

namespace ZiziBot.Application.Common.Constants;

public static class ValueConst
{
    public const string DB_SQLITE_CONNECTION_STRING = "Data Source=Storage/Sqlite/Data.db";

    public const ExecutionStrategy DEFAULT_EXECUTION_STRATEGY = ExecutionStrategy.Instant;

    public const int NEW_MEMBER_RAID_WINDOW_LIMIT = 7;
    public readonly static TimeSpan NEW_MEMBER_RAID_SLIDING_WINDOW = TimeSpan.FromMinutes(5);
    public readonly static TimeSpan NEW_MEMBER_RAID_MODE_MUTE_DURATION = TimeSpan.FromHours(1);

    public readonly static TimeSpan NEW_MEMBER_ENTRY_SAFE_LIMIT = TimeSpan.FromDays(3);

    public readonly static char[] WordSeparator = [' ', '\n', ':', ';', ','];

    public static string UniqueKey { get; set; } = "";

    public const string NEW_MEMBER_DEFAULT_WELCOME_MESSAGE = "Hai {users}" +
                                                             "\nSelamat datang di Kontrakan <b>{ChatTitle}</b>";

    public const int CENDOL_PRICE = 5000;
    public const string TRAKTEER_PAYMENT = "https://trakteer.id/payment-status";

    public readonly static long[] SAFE_IDS = [
        777000
    ];

    public readonly static IEnumerable<int> PaginationSize = [
        10,
        20,
        50,
        100,
        200,
        500,
        1000
    ];

    public const string USER_INFO = "UserInfo";
}