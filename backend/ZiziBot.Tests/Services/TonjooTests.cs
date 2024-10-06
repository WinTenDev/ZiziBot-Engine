using FluentAssertions;
using Xunit;

namespace ZiziBot.Tests.Services;

public class TonjooTests(TonjooService tonjooService)
{
    [Theory]
    [InlineData("jne", "8825112045716759")]
    public async Task GetAwbRawTest(string courier, string awb)
    {
        var checkAwb = await tonjooService.GetAwbInfoRaw(courier, awb);

        checkAwb.Data?.Found.Should().BeTrue();
    }

    [Theory]
    [InlineData("jne", "8825112045716759")]
    public async Task GetAwbMergedTest(string courier, string awb)
    {
        var checkAwb = await tonjooService.GetAwbInfoMerged(courier, awb);

        checkAwb.Should().NotBeNullOrEmpty();
    }
}