namespace ZiziBot.Application.Common.Constants;

public static class Env
{
    public const string AZURE_APP_CONFIG_CONNECTION_STRING = "AZURE_APP_CONFIG_CONNECTION_STRING";
    public const string MONGODB_CONNECTION_STRING = "MONGODB_CONNECTION_STRING";
    public const string TELEGRAM_WEBHOOK_URL = "TELEGRAM_WEBHOOK_URL";

    public const string WEB_CONSOLE_URL = "WEB_CONSOLE_URL";
    public const string DASHBOARD_PROJECT_PATH = "./../ng-dashboard";
    public const string DASHBOARD_DIST_PATH = $"{DASHBOARD_PROJECT_PATH}/dist/ng-dashboard";

    public const string COMMON_UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36";

    public const int DEFAULT_TIMEZONE = 7;

    public static string? BotToken { get; set; }
    public static string? SentryDsn { get; set; }
    public static string? GithubToken { get; set; }
}