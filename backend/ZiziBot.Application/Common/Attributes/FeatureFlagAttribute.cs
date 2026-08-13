namespace ZiziBot.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class FeatureFlagAttribute(string featureName) : Attribute
{
    public string FeatureName { get; } = featureName;
}