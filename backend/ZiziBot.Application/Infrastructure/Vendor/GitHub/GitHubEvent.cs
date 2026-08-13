using System.Text.Json.Serialization;

namespace ZiziBot.Application.Infrastructure.Vendor.GitHub;

public partial class GitHubEvent
{
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("check_suite")]
    public CheckSuite CheckSuite { get; set; }

    [JsonPropertyName("ref")]
    public string Ref { get; set; }

    [JsonPropertyName("before")]
    public string Before { get; set; }

    [JsonPropertyName("after")]
    public string After { get; set; }

    [JsonPropertyName("repository")]
    public Repository Repository { get; set; }

    [JsonPropertyName("pusher")]
    public Pusher Pusher { get; set; }

    [JsonPropertyName("organization")]
    public Organization Organization { get; set; }

    [JsonPropertyName("sender")]
    public Sender Sender { get; set; }

    [JsonPropertyName("created")]
    public bool Created { get; set; }

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }

    [JsonPropertyName("forced")]
    public bool Forced { get; set; }

    [JsonPropertyName("base_ref")]
    public object BaseRef { get; set; }

    [JsonPropertyName("compare")]
    public string Compare { get; set; }

    [JsonPropertyName("commits")]
    public List<Commit> Commits { get; set; }

    [JsonPropertyName("head_commit")]
    public Commit HeadCommit { get; set; }
}