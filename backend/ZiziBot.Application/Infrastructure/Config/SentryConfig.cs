using System.ComponentModel;

namespace ZiziBot.Application.Infrastructure.Config;

[DisplayName("Sentry")]
public class SentryConfig
{
    public bool IsEnabled { get; set; }
    public string Dsn { get; set; }
}