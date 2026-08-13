using AngleSharp.Html.Dom;
using ZiziBot.Application.Common.Utils;
using ZiziBot.Application.Infrastructure.Vendor.JadwalSholatOrg;

namespace ZiziBot.Application.Infrastructure.WebParser;

public static class JadwalSholatOrgParserUtil
{
    private const string WEB_URL = "https://jadwalsholat.org/jadwal-sholat/monthly.php";

    public static async Task<List<City>?> GetCities()
    {
        var document = await WEB_URL.OpenUrl();

        var cities = document?.QuerySelector<IHtmlSelectElement>("select[class=town-select]")?.Options.Select(x => new City() {
            CityId = x.Value.Convert<int>(),
            CityCode = x.Text.ToLower().RegexReplace("[^a-zA-Z]", ""),
            CityName = x.Text
        }).ToList();

        return cities;
    }

    public static async Task<List<List<ShalatTime>?>> FetchSchedulesFullYear(int cityId)
    {
        var year = DateTime.UtcNow.Year;
        var months = Enumerable.Range(1, 12);

        var monthTask = await months.Select(x => FetchSchedules(cityId, x, year)).WhenAll(1);

        return monthTask.ToList();
    }

    public static async Task<List<ShalatTime>?> FetchSchedules(int cityId)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;

        var tableRows = await FetchSchedules(cityId, month, year);

        return tableRows;
    }

    public static async Task<List<ShalatTime>?> FetchSchedules(int cityId, int month)
    {
        var year = DateTime.UtcNow.Year;

        return await FetchSchedules(cityId, month, year);
    }


    public static async Task<List<ShalatTime>?> FetchSchedules(int cityId, int month, int year)
    {
        var webUrl = $"{WEB_URL}?id={cityId}&m={month}&y={year}";
        var document = await webUrl.OpenUrl();

        var tableRows = document?.QuerySelectorAll<IHtmlTableRowElement>("table[class=table_adzan] > tbody > tr");
        var times = tableRows?.Skip(2)
            .Select(x => x.Cells.Select(y => y.TextContent.RegexMatchIf("[0-9][0-9]:[0-9][0-9]+", y.TextContent.Contains(':')))).ToList();

        var header = times?.FirstOrDefault();

        var shalatTimes = times?.Skip(1)
            .Select(row => header?.Zip(row, (h, t) => new { h, t }).ToDictionary(ht => ht.h, ht => ht.t))
            .SkipLast(13)
            .Select(row => {
                if (row == null) return new();

                var date = row.GetValueOrDefault("Tanggal");

                var time = new ShalatTime {
                    Date = new DateOnly(year, month, date.Convert<int>()),
                    Fajr = row.GetValueOrDefault("Shubuh").ToTimeOnly(),
                    Sunrise = row.GetValueOrDefault("Terbit").ToTimeOnly(),
                    Dhuhr = row.GetValueOrDefault("Dzuhur").ToTimeOnly(),
                    Ashr = row.GetValueOrDefault("Ashr").ToTimeOnly(),
                    Maghrib = row.GetValueOrDefault("Maghrib").ToTimeOnly(),
                    Isha = row.GetValueOrDefault("Isya").ToTimeOnly()
                };

                return time;
            }).ToList();

        return shalatTimes;
    }
}