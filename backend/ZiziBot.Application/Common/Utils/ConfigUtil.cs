using Serilog.Events;
using ZiziBot.Application.Infrastructure.Config;
using ZiziBot.Application.Common.Dtos;
using ZiziBot.Application.Common.Enums;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.Application.Common.Utils;

public class ConfigUtil
{
    public static List<SeedSettingDto> GetAppDefault()
    {
        var engine = new EngineConfig
        {
            ProductName = "ZiziBot",
            Description = "ZiziBot is a Telegram bot that can help you manage your group.",
            Vendor = "@AZCorp",
            Website = "https://azhe.my.id",
            Support = "https://azhe.my.id",
            TelegramEngineMode = BotEngineMode.Auto,
            ExecutionMode = ExecutionMode.Await,
            EnableChatRestriction = false
        };

        var jwt = new JwtConfig()
        {
            Key = "YOUR_SECURE_SECRET_KEY",
            Audience = "YOUR_AUDIENCE",
            Issuer = "YOUR_ISSUER",
        };

        var eventLog = new EventLogConfig
        {
            ChatId = 12345,
            ThreadId = 34567,
            BackupDB = 0,
            Exception = 0,
            EventLog = 0,
            LogLevel = LogEventLevel.Debug,
            ProcessEnrich = false,
            WriteToFile = true,
            WriteToSignalR = false,
            WriteToTelegram = false,
        };

        var cache = new CacheConfig()
        {
            PrefixRoot = "zizibot",
            UseJsonFile = false,
            UseFirebase = false,
            UseMongoDb = false,
            UseRedis = false,
            UseSqlite = false,
            RedisConnection = "localhost:6379",
            FirebaseProjectUrl = "SOME_FIREBASE_PROJECT_URL",
            FirebaseServiceAccountJson = "SOME_FIREBASE_SA_JSON",
            CacheEngine = CacheEngine.FusionCache
        };

        var hangfire = new HangfireConfig()
        {
            CurrentStorage = CurrentStorage.InMemory,
            DashboardTitle = "Zizi Dev - Hangfire Dashboard",
            MongoDbConnection = "mongo://localhost:21750",
            WorkerMultiplier = 2
        };

        var mirror = new MirrorConfig()
        {
            ApprovalChannelId = 12345,
            PaymentExpirationDays = 3,
            SaweriaVerificationApi = "",
            TrakteerVerificationApi = "",
            TrakteerWebHookToken = "YOUR_TRAKTEER_WEBHOOK_TOKEN",
            UseCustomSaweriaApi = false,
            UseCustomTrakteerApi = false,
        };

        var pendekin = new PendekinConfig()
        {
            RouterBaseUrl = "127.0.0.1:7140"
        };

        var sentry = new SentryConfig()
        {
            IsEnabled = false,
            Dsn = "SENTRY_DSN"
        };

        var gcp = new GcpConfig()
        {
            IsEnabled = false,
            FirebaseProjectUrl = "yourapp.firebaseio.com",
            FirebaseServiceAccountJson = "{\"your_firebase_service_account_json\":\"string\"}"
        };

        var optiicDev = new OptiicDevConfig()
        {
            IsEnabled = false,
            ApiKey = "YOUR_API_KEY"
        };

        var binderByte = new BinderByteConfig()
        {
            IsEnabled = false,
            BaseUrl = "api.your-domain.com",
            ApiKey = "YOUR_API_KEY"
        };

        var seedAppSettings = new List<SeedSettingDto>
        {
            new()
            {
                Root = nameof(ConfigRoot.Engine),
                KeyPair = engine.ToDictionary(StringType.PascalCase)
            },
            new()
            {
                Root = nameof(ConfigRoot.Jwt),
                KeyPair = jwt.ToDictionary(StringType.PascalCase)
            },
            new()
            {
                Root = nameof(ConfigRoot.EventLog),
                KeyPair = eventLog.ToDictionary(StringType.PascalCase)
            },
            new()
            {
                Root = nameof(ConfigRoot.Hangfire),
                KeyPair = hangfire.ToDictionary(StringType.PascalCase)
            },
            new()
            {
                Root = "Flag",
                KeyPair = new()
                {
                    {
                        "IsEnabled", false
                    },
                    {
                        "IsForwardMessageEnabled", false
                    }
                }
            },
            new()
            {
                Root = nameof(ConfigRoot.Cache),
                KeyPair = cache.ToDictionary(StringType.PascalCase)
            },
            new()
            {
                Root = nameof(ConfigRoot.Sentry),
                KeyPair = sentry.ToDictionary(StringType.PascalCase)
            },
            new()
            {
                Root = nameof(ConfigRoot.Mirror),
                KeyPair = mirror.ToDictionary(StringType.PascalCase)
            },
            new()
            {
                Root = nameof(ConfigRoot.Pendekin),
                KeyPair = pendekin.ToDictionary(StringType.PascalCase)
            },
            new()
            {
                Root = nameof(ConfigRoot.Gcp),
                KeyPair = gcp.ToDictionary(StringType.PascalCase)
            },
            new()
            {
                Root = nameof(ConfigRoot.OptiicDev),
                KeyPair = optiicDev.ToDictionary(StringType.PascalCase)
            },
            new()
            {
                Root = nameof(ConfigRoot.BinderByte),
                KeyPair = binderByte.ToDictionary(StringType.PascalCase)
            }
        };

        return seedAppSettings;
    }
}