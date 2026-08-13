namespace ZiziBot.Attributes;

public class StartupTaskAttribute : Attribute
{
    public int Order { get; set; } = 1;
    public bool SkipAwait { get; set; } = false;
}