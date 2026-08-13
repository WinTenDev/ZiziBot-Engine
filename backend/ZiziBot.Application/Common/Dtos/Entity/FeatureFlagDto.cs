namespace ZiziBot.Application.Common.Dtos.Entity;

public class FeatureFlagDto : EntityDtoBase
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public bool IsEnabled { get; set; }
}