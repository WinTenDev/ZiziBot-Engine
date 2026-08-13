namespace ZiziBot.Application.Common.Constants;

public static class PathConst
{
    private static readonly string Storage = Path.Combine(Environment.CurrentDirectory, "Storage");
    private static readonly string CacheTower = Path.Combine(Storage, "CacheTower");
    private static readonly string FusionCache = Path.Combine(Storage, "FusionCache");
    private static readonly string HangfirePath = Path.Combine(Storage, "Hangfire");

    public static readonly string TEMP_PATH = Path.Combine(Storage, "Temp");

    public static readonly string CACHE_TOWER_JSON = Path.Combine(CacheTower, "Json");
    public static readonly string CACHE_TOWER_SQLITE = Path.Combine(CacheTower, "Sqlite", "Sqlite.db");

    public static readonly string FUSION_CACHE_SQLITE = Path.Combine(FusionCache, "Sqlite", "Sqlite.db");

    public static readonly string HANGFIRE_SQLITE_PATH = Path.Combine(HangfirePath, "Sqlite", "Sqlite.db");
    public static readonly string HANGFIRE_LITEDB_PATH = Path.Combine(HangfirePath, "LiteDb", "LiteDb.db");

    public static readonly string BACKUP = Path.Combine(Storage, "Backup");
    public static readonly string LOG = Path.Combine(Storage, "Log");
    public static readonly string MONGODB_BACKUP = Path.Combine(BACKUP, "MongoDB");
}