using System.Reflection;

namespace ZiziBot.Application.Common.Utils;

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

    public static Version GetVersionNumber()
    {
        var currentAssembly = Assembly.GetEntryAssembly()?.GetName();
        var version = currentAssembly?.Version ?? new Version(26, 2, 0, 0);
        return version;
    }

    public static string GetVersion(bool pretty = false)
    {
        var currentAssembly = Assembly.GetEntryAssembly()?.GetName();
        var version = currentAssembly?.Version ?? new Version(26, 2, 0, 0);

        return pretty ? $"{version.Major}.{version.Minor} Build {version.Build}" : version.ToString();
    }

    public static DateTime GetBuildDate()
    {
        // var attribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<BuildStampAttribute>();
        return File.GetCreationTimeUtc(Assembly.GetExecutingAssembly().Location);
    }
}