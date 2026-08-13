using System.Text.Json.Serialization;

namespace ZiziBot.Application.Infrastructure.Vendor.GitLab;

public partial class GitLabEvent
{
    [JsonPropertyName("object_kind")]
    public string ObjectKind { get; set; }

    [JsonPropertyName("event_name")]
    public string EventName { get; set; }

    [JsonPropertyName("before")]
    public string Before { get; set; }

    [JsonPropertyName("after")]
    public string After { get; set; }

    [JsonPropertyName("ref")]
    public string Ref { get; set; }

    [JsonPropertyName("ref_protected")]
    public bool RefProtected { get; set; }

    [JsonPropertyName("checkout_sha")]
    public string CheckoutSha { get; set; }

    [JsonPropertyName("message")]
    public object Message { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("user_name")]
    public string UserName { get; set; }

    [JsonPropertyName("user_username")]
    public string UserUsername { get; set; }

    [JsonPropertyName("user_email")]
    public string UserEmail { get; set; }

    [JsonPropertyName("user_avatar")]
    public Uri UserAvatar { get; set; }

    [JsonPropertyName("project_id")]
    public long ProjectId { get; set; }

    [JsonPropertyName("project")]
    public Project Project { get; set; }

    [JsonPropertyName("commits")]
    public List<GitLabCommit> Commits { get; set; }

    [JsonPropertyName("total_commits_count")]
    public long TotalCommitsCount { get; set; }

    [JsonPropertyName("push_options")]
    public PushOptions PushOptions { get; set; }

    [JsonPropertyName("repository")]
    public Repository Repository { get; set; }
}

public partial class GitLabCommit
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("author")]
    public Author Author { get; set; }

    [JsonPropertyName("added")]
    public List<string> Added { get; set; }

    [JsonPropertyName("modified")]
    public List<string> Modified { get; set; }

    [JsonPropertyName("removed")]
    public List<object> Removed { get; set; }
}

public partial class Author
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}

public partial class Project
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("web_url")]
    public string WebUrl { get; set; }

    [JsonPropertyName("avatar_url")]
    public object AvatarUrl { get; set; }

    [JsonPropertyName("git_ssh_url")]
    public string GitSshUrl { get; set; }

    [JsonPropertyName("git_http_url")]
    public string GitHttpUrl { get; set; }

    [JsonPropertyName("namespace")]
    public string Namespace { get; set; }

    [JsonPropertyName("visibility_level")]
    public long VisibilityLevel { get; set; }

    [JsonPropertyName("path_with_namespace")]
    public string PathWithNamespace { get; set; }

    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; set; }

    [JsonPropertyName("ci_config_path")]
    public string CiConfigPath { get; set; }

    [JsonPropertyName("homepage")]
    public Uri Homepage { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("ssh_url")]
    public string SshUrl { get; set; }

    [JsonPropertyName("http_url")]
    public Uri HttpUrl { get; set; }
}

public partial class PushOptions
{
}

public partial class Repository
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("homepage")]
    public Uri Homepage { get; set; }

    [JsonPropertyName("git_http_url")]
    public Uri GitHttpUrl { get; set; }

    [JsonPropertyName("git_ssh_url")]
    public string GitSshUrl { get; set; }

    [JsonPropertyName("visibility_level")]
    public long VisibilityLevel { get; set; }
}