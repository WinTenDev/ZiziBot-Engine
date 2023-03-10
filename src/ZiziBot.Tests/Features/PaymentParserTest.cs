using FluentAssertions;
using Xunit;

namespace ZiziBot.Tests.Pipelines;

public class MirrorUserParserTest
{
    public MirrorUserParserTest(MediatorService mediatorService)
    {

    }

    [Theory]
    [InlineData("https://trakteer.id/payment-status/ca9c28da-87c0-5d32-9b6f-a220d3d36dfd")]
    public async Task TrakteerParserTest(string url)
    {
        var requiredNodes = await url.ParseTrakteerWeb();

        requiredNodes.RawText.Should().Contain("Pembayaran Berhasil");
        requiredNodes.Cendols.Should().NotBeNullOrEmpty();
        requiredNodes.AdminFees.Should().NotBeNullOrEmpty();
        requiredNodes.Subtotal.Should().NotBeNullOrEmpty();
        requiredNodes.OrderDate.Should().BeMoreThan(default);
        requiredNodes.OrderId.Should().NotBeNullOrEmpty();
        requiredNodes.PaymentMethod.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("https://trakteer.id/payment-status/ca9c28da-87c0-5d32-9b6f-a220d3d36dfdX")]
    public async Task TrakteerParserNegativeTest(string url)
    {
        var requiredNodes = await url.ParseTrakteerWeb();
        requiredNodes.RawText.Should().BeNullOrEmpty();
    }
}