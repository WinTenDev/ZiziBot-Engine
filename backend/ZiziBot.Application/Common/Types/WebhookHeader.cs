using Microsoft.Extensions.Primitives;
using ZiziBot.Application.Common.Utils;

namespace ZiziBot.Application.Common.Types;

public class WebhookHeader
{
    public string? Host { get; init; }
    public string? UserAgent { get; init; }
    public string? ContentType { get; init; }
    public string? Delivery { get; init; }
    public string? Event { get; init; }
    public string? HookId { get; init; }
    public string? HookInstallationTargetId { get; init; }
    public string? HookInstallationTargetType { get; init; }
    public StringValues SonarQubeProject { get; set; }

    public static WebhookHeader? Parse(IDictionary<string, StringValues> headers)
    {
        if (headers.IsEmpty())
            return null;

        var webhookHeader = new WebhookHeader {
            Host = headers.FirstOrDefault(kv => kv.Key.Like("host")).Value.ToString(),
            UserAgent = headers.FirstOrDefault(kv => kv.Key.Like("user-agent")).Value.ToString(),
            ContentType = headers.FirstOrDefault(kv => kv.Key.Like("content-type")).Value.ToString(),
            Delivery = headers.FirstOrDefault(kv => kv.Key.Like("delivery")).Value.ToString(),
            Event = headers.FirstOrDefault(kv => kv.Key.Like("event")).Value.ToString(),
            HookId = headers.FirstOrDefault(kv => kv.Key.Like("hook-id")).Value.ToString(),
            HookInstallationTargetId = headers.FirstOrDefault(kv => kv.Key.Like("hook-installation-target-id")).Value.ToString(),
            HookInstallationTargetType = headers.FirstOrDefault(kv => kv.Key.Like("hook-installation-target-type")).Value.ToString(),
            SonarQubeProject = headers.FirstOrDefault(kv => kv.Key.Like("SonarQube-Project")).Value.ToString()
        };

        return webhookHeader;
    }
}