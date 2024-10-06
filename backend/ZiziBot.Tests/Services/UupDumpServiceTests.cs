using FluentAssertions;
using Flurl.Http;
using Xunit;

namespace ZiziBot.Tests.Services;

public class UupDumpServiceTests(UupDumpService uupDumpService)
{
    public static readonly object[][] ShalatCity = {
        new object[] { DateOnly.FromDateTime(DateTime.Now), 712 },
        new object[] { new DateOnly(2017, 3, 1), 712 },
    };

    [SkippableTheory(typeof(FlurlHttpException), typeof(FlurlParsingException))]
    [InlineData("")]
    [InlineData("19041")]
    [InlineData("22000")]
    public async Task GetBuildTest(string search)
    {
        var updates = await uupDumpService.GetUpdatesAsync(search);

        updates.Should().NotBeNull();
    }
}