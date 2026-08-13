namespace ZiziBot.Application.Common.Types;

public class BotApiDoc
{
    public string Href { get; set; }
    public string? Text { get; set; }
    public List<ApiDocField>? Fields { get; set; }
}

public class ApiDocField
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
}