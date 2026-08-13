using System.Text.Json.Serialization;

namespace ZiziBot.Application.Infrastructure.Vendor.GitHub;

public partial class CheckSuite
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    [JsonPropertyName("head_branch")]
    public string HeadBranch { get; set; }

    [JsonPropertyName("head_sha")]
    public string HeadSha { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("conclusion")]
    public string Conclusion { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("before")]
    public string Before { get; set; }

    [JsonPropertyName("after")]
    public string After { get; set; }

    [JsonPropertyName("pull_requests")]
    public List<object> PullRequests { get; set; }

    [JsonPropertyName("app")]
    public App App { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("rerequestable")]
    public bool Rerequestable { get; set; }

    [JsonPropertyName("runs_rerequestable")]
    public bool RunsRerequestable { get; set; }

    [JsonPropertyName("latest_check_runs_count")]
    public long LatestCheckRunsCount { get; set; }

    [JsonPropertyName("check_runs_url")]
    public Uri CheckRunsUrl { get; set; }

    [JsonPropertyName("head_commit")]
    public HeadCommit HeadCommit { get; set; }
}