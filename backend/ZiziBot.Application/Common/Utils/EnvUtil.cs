using Serilog;
using ZiziBot.Application.Common.Constants;
using ZiziBot.Application.Common.Dtos;
using ZiziBot.Application.Common.Exceptions;

namespace ZiziBot.Application.Common.Utils;

public static class EnvUtil
{
    public static List<FlagDto>? Current { get; set; }
    private static readonly ILogger Logger = Log.ForContext(typeof(EnvUtil));


    public static string GetEnv(string key, string? defaultValue = null, bool throwIsMissing = false)
    {
        var envVal = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        if (!string.IsNullOrEmpty(envVal))
            return envVal;

        if (throwIsMissing)
            throw new EnvMissingException(key);

        return defaultValue ?? string.Empty;
    }

    public static bool IsEnvExist(string key)
    {
        var envVal = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        return !string.IsNullOrEmpty(envVal);
    }

    public static bool IsDevelopment()
    {
        return IsEnv("ASPNETCORE_ENVIRONMENT", "Development");
    }

    public static bool IsStaging()
    {
        return IsEnv("ASPNETCORE_ENVIRONMENT", "Staging");
    }

    public static bool IsProduction()
    {
        return IsEnv("ASPNETCORE_ENVIRONMENT", "Production");
    }

    public static bool IsEnv(string key, string value)
    {
        var envVal = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        return string.Equals(envVal, value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsEnabled(string flagName)
    {
        var flag = Current?.FirstOrDefault(x => x.Name == flagName);

        if (flag == null)
        {
            var defaultFlag = Flag.GetFields().FirstOrDefault(x => x.Name == flagName);
            var flagValue = defaultFlag?.Value ?? false;
            Logger.Verbose("Flag {FlagName} not found. Using default value: {DefaultFlagValue}", flagName, flagValue);

            return flagValue;
        }

        Logger.Verbose("Flag {FlagName} is {FlagValue}", flag.Name, flag.Value);
        return flag.Value;
    }
}