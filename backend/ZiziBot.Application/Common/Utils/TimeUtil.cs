using System.Globalization;
using Humanizer;
using TimeSpanParserUtil;

namespace ZiziBot.Application.Common.Utils;

public static class TimeUtil
{
    public static string GenerateVersion(this DateTime dateTime)
    {
        var timeToday = dateTime.ToString("h:mm:ss");
        var jan2000 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var majorNumber = dateTime.Year.ToString().Replace("0", "");
        var minorNumber = dateTime.Month;
        var buildNumber = (dateTime - jan2000).Days;
        var revNumber = TimeSpan.ParseExact(timeToday, @"h\:mm\:ss", CultureInfo.InvariantCulture).TotalSeconds;
        var projectVersion = $"{majorNumber}.{minorNumber}.{buildNumber}.{revNumber}";

        return projectVersion;
    }

    public static string GetDelay(this DateTime dateTime)
    {
        var dateNow = DateTime.UtcNow;
        var span = dateNow - dateTime;
        var delay = span.ToString(@"s\,fff");
        return delay;
    }

    public static TimeSpan ParseDateTime(this string dateTime)
    {
        return TimeSpanParser.Parse(dateTime);
    }

    public static string ForHuman(this TimeSpan timeSpan, int precision = 10, string cultureInfo = "id-id")
    {
        return timeSpan.Humanize(precision: precision, maxUnit: TimeUnit.Year, culture: CultureInfo.GetCultureInfo(cultureInfo));
    }

    public static string GetTimeGreet()
    {
        var greet = "dini hari";
        var hour = DateTime.Now.Hour;

        greet = hour switch
        {
            <= 3 => "dini hari",
            <= 10 => "pagi",
            <= 14 => "siang",
            <= 17 => "sore",
            <= 18 => "petang",
            <= 24 => "malam",
            _ => greet
        };

        return greet;
    }

    public static TimeOnly ToTimeOnly(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return default;

        return TimeOnly.Parse(input, new DateTimeFormatInfo());
    }

    #region Cron

    /// <summary>
    /// Returns cron expression that fires every &lt;<paramref name="interval"></paramref>&gt; minutes.
    /// </summary>
    /// <param name="interval">The number of minutes to wait between every activation.</param>
    public static string MinuteInterval(int interval) => $"*/{interval} * * * *";

    /// <summary>
    /// Returns cron expression that fires every &lt;<paramref name="interval"></paramref>&gt; hours.
    /// </summary>
    /// <param name="interval">The number of hours to wait between every activation.</param>
    public static string HourInterval(int interval) => $"0 */{interval} * * *";

    /// <summary>
    /// Returns cron expression that fires every &lt;<paramref name="interval"></paramref>&gt; days.
    /// </summary>
    /// <param name="interval">The number of days to wait between every activation.</param>
    public static string DayInterval(int interval) => $"0 0 */{interval} * *";

    /// <summary>
    /// Returns cron expression that fires every &lt;<paramref name="interval"></paramref>&gt; months.
    /// </summary>
    /// <param name="interval">The number of months to wait between every activation.</param>
    public static string MonthInterval(int interval) => $"0 0 1 */{interval} *";

    #endregion Cron
}