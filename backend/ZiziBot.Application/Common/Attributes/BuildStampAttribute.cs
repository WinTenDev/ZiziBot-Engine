using System.Globalization;

namespace ZiziBot.Attributes;

public class BuildStampAttribute : Attribute
{
    public DateTime BuildDate { get; set; }
    public Version Version { get; set; }

    public BuildStampAttribute(string value)
    {
        BuildDate = DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
        Version = Version.Parse(GenerateVersion(BuildDate));
    }

    public static string GenerateVersion(DateTime dateTime)
    {
        var timeToday = dateTime.ToString("h:mm:ss");
        var jan2000 = new DateTime(2000, 1, 1);

        var majorNumber = dateTime.Year.ToString().Replace("0", "");
        var minorNumber = dateTime.Month;
        var buildNumber = (dateTime - jan2000).Days;
        var revNumber = TimeSpan.Parse(timeToday).TotalSeconds;
        var projectVersion = $"{majorNumber}.{minorNumber}.{buildNumber}.{revNumber}";

        return projectVersion;
    }
}