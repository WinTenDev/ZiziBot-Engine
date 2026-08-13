using Xunit;
using ZiziBot.Application.Common.Utils;

namespace ZiziBot.Tests.Features;

public class UrlUtilTests
{
    [Theory]
    [InlineData("https://github.com/azhe403/ZiziBot-Engine")]
    public void GetUrlSegmentTest(string url)
    {
        // Arrange
        var parse0 = url.UrlSegment(0);
        var parse1 = url.UrlSegment(1);

        // Assert
        parse0.ShouldBe("azhe403");
        parse1.ShouldBe("ZiziBot-Engine");
    }
}