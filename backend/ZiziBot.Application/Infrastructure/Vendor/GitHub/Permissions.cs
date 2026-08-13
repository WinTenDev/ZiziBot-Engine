using System.Text.Json.Serialization;

namespace ZiziBot.Application.Infrastructure.Vendor.GitHub;

public partial class Permissions
{
    [JsonPropertyName("checks")]
    public string Checks { get; set; }

    [JsonPropertyName("contents")]
    public string Contents { get; set; }

    [JsonPropertyName("emails")]
    public string Emails { get; set; }

    [JsonPropertyName("members")]
    public string Members { get; set; }

    [JsonPropertyName("metadata")]
    public string Metadata { get; set; }

    [JsonPropertyName("pull_requests")]
    public string PullRequests { get; set; }

    [JsonPropertyName("security_events")]
    public string SecurityEvents { get; set; }

    [JsonPropertyName("statuses")]
    public string Statuses { get; set; }
}