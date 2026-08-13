namespace ZiziBot.Application.Common.Types;

public class RailwayWebhook
{
    public string Type { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public Project Project { get; set; }
    public Environment Environment { get; set; }
    public Deployment Deployment { get; set; }
}

public class Deployment
{
    public Guid Id { get; set; }
    public Creator Creator { get; set; }
    public Meta Meta { get; set; }
}

public class Creator
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Uri Avatar { get; set; }
}

public class Meta
{
}

public class Environment
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}