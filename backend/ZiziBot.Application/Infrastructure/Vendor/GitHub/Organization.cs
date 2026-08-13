using System.Text.Json.Serialization;

namespace ZiziBot.Application.Infrastructure.Vendor.GitHub;

public partial class Organization
{
    [JsonPropertyName("login")]
    public string Login { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("repos_url")]
    public Uri ReposUrl { get; set; }

    [JsonPropertyName("events_url")]
    public Uri EventsUrl { get; set; }

    [JsonPropertyName("hooks_url")]
    public Uri HooksUrl { get; set; }

    [JsonPropertyName("issues_url")]
    public Uri IssuesUrl { get; set; }

    [JsonPropertyName("members_url")]
    public string MembersUrl { get; set; }

    [JsonPropertyName("public_members_url")]
    public string PublicMembersUrl { get; set; }

    [JsonPropertyName("avatar_url")]
    public Uri AvatarUrl { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}