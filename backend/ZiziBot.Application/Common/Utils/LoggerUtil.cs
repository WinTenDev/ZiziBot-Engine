using Serilog;
using Serilog.Core;

namespace ZiziBot.Application.Common.Utils;

public static class LoggerUtil
{
    public static ILogger CreateLogger<T>()
    {
        return Log.ForContext(typeof(T));
    }

    public static ILogger CreateLogger(Type type)
    {
        return Log.ForContext(type);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void LogError(string messageTemplate, params object?[]? propertyValues)
    {
        Log.Error(messageTemplate, propertyValues);
    }
}