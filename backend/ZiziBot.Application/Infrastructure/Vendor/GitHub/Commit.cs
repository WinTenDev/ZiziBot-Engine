using System.Text.Json.Serialization;

namespace ZiziBot.Application.Infrastructure.Vendor.GitHub;

public partial class Commit
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("tree_id")]
    public string TreeId { get; set; }

    [JsonPropertyName("distinct")]
    public bool Distinct { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("author")]
    public Author Author { get; set; }

    [JsonPropertyName("committer")]
    public Author Committer { get; set; }

    [JsonPropertyName("added")]
    public List<object> Added { get; set; }

    [JsonPropertyName("removed")]
    public List<object> Removed { get; set; }

    [JsonPropertyName("modified")]
    public List<string> Modified { get; set; }
}