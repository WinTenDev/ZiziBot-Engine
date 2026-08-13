using ZiziBot.Application.Common.Enums;

namespace ZiziBot.Application.Common.Types;

public class BotPipelineResult
{
    public bool HasUsername { get; set; }
    public bool IsMessagePassed { get; set; }
    public bool IsUserPassed { get; set; }
    public bool IsChatPassed { get; set; }
    public bool IsRolePassed { get; set; }
    public List<PipelineResultAction> Actions { get; set; } = [];
}