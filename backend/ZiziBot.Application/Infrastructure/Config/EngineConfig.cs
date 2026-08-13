using System.ComponentModel;
using ZiziBot.Application.Common.Enums;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.Application.Infrastructure.Config;

[DisplayName("Engine")]
public class EngineConfig
{
    public string ProductName { get; set; }
    public string Description { get; set; }
    public string Vendor { get; set; }
    public string Website { get; set; }
    public string Support { get; set; }
    public string WebhookUrl { get; set; }
    public string ConsoleUrl { get; set; }
    public BotEngineMode TelegramEngineMode { get; set; }
    public ExecutionMode ExecutionMode { get; set; }
    public bool EnableChatRestriction { get; set; }
    public FeatureFlagEngine FeatureFlagEngine { get; set; } = FeatureFlagEngine.Internal;
}