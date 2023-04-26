using System.Reflection;

namespace ZiziBot.Parsers;

public static class VersionUtil
{
    public static int GetBuildNumber()
    {
        var jan2000 = new DateTime(2000, 1, 1);
        var today = DateTime.UtcNow;
        var diffDay = (today - jan2000).Days;

        return diffDay;
    }

    public static double GetRevNumber()
    {
        var dateNow = DateTime.UtcNow.ToString("h:mm:ss");
        var seconds = TimeSpan.Parse(dateNow).TotalSeconds;
        return seconds;
    }

    public static string GetVersion(bool pretty = false)
    {
        var currentAssembly = Assembly.GetEntryAssembly()?.GetCustomAttribute<BuildStampAttribute>();
        var version = currentAssembly?.Version ?? new Version(1, 0, 0, 0);

        return pretty ? $"{version.Major}.{version.Minor} Build {version.Build}" : version.ToString();
    }

    public static DateTime GetBuildDate()
    {
        var attribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<BuildStampAttribute>();
        return attribute?.BuildDate ?? default(DateTime);
    }
}