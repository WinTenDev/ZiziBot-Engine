using System.Threading;

namespace ZiziBot.Application.Infrastructure.Pipelines;

internal static class PipelineMetrics
{
    private static readonly AsyncLocal<string?> SessionIdHolder = new();

    public static string? CurrentSessionId
    {
        get => SessionIdHolder.Value;
        set => SessionIdHolder.Value = value;
    }

    public static string GetTypeName(Type type)
    {
        var typeName = type.Name;
        var genericMarkerIndex = typeName.IndexOf('`');

        return genericMarkerIndex >= 0
            ? typeName[..genericMarkerIndex]
            : typeName;
    }
}
