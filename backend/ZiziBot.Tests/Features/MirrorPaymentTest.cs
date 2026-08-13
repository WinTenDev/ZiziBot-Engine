using Xunit;

namespace ZiziBot.Tests.Features;

public class MirrorPaymentTest(MediatorService mediatorService, MirrorPaymentRestService mirrorPaymentRestService)
{
    [Theory]
    [InlineData("ca9c28da-87c0-5d32-9b6f-a220d3d36dfd")] // trakteer
    [InlineData("65190576-e653-47d2-b472-9a367a54ed23")] // saweria
    public async Task ParseDonationTest(string url)
    {
        var donationParsedDto = await mirrorPaymentRestService.ParseTrakteerWeb(url);

        if (donationParsedDto.IsValid)
        {
            donationParsedDto.CendolCount.ShouldBeGreaterThan(0);
            donationParsedDto.OrderDate.ShouldBeGreaterThan(default);
            donationParsedDto.OrderId.ShouldNotBeNullOrEmpty();
        }
    }

    [Theory]
    [InlineData("ca9c28da-87c0-5d32-9b6f-a220d3d36dfd")]
    [InlineData("https://trakteer.id/payment-status/ca9c28da-87c0-5d32-9b6f-a220d3d36dfd")]
    public async Task TrakteerParserTest(string url)
    {
        var trakteerParsedDto = await mirrorPaymentRestService.ParseTrakteerWeb(url);

        if (trakteerParsedDto.IsValid)
        {
            trakteerParsedDto.RawText.ShouldNotBeNull().ShouldContain("Pembayaran Berhasil");
            trakteerParsedDto.Cendols.ShouldNotBeNullOrEmpty();
            trakteerParsedDto.AdminFees.ShouldBeGreaterThan(0);
            trakteerParsedDto.Subtotal.ShouldBeGreaterThan(0);
            trakteerParsedDto.OrderDate.ShouldBeGreaterThan(default);
            trakteerParsedDto.OrderId.ShouldNotBeNullOrEmpty();
            trakteerParsedDto.PaymentMethod.ShouldNotBeNullOrEmpty();
        }
    }

    [Theory]
    [InlineData("https://trakteer.id/payment-status/ca9c28da-87c0-5d32-9b6f-a220d3d36dfdX")]
    public async Task TrakteerParserNegativeTest(string url)
    {
        var requiredNodes = await mirrorPaymentRestService.ParseTrakteerWeb(url);
        requiredNodes.RawText.ShouldBeNullOrEmpty();
    }

    [Theory]
    [InlineData("ca9c28da-87c0-5d32-9b6f-a220d3d36dfd")]
    [InlineData("https://trakteer.id/payment-status/ca9c28da-87c0-5d32-9b6f-a220d3d36dfd")]
    public async Task TrakteerApiTest(string url)
    {
        var trakteerApi = await mirrorPaymentRestService.GetTrakteerApi(url);

        if (trakteerApi.IsValid)
        {
            trakteerApi.OrderId.ShouldNotBeNullOrEmpty();
            trakteerApi.CendolCount.ShouldBeGreaterThan(0);
            trakteerApi.AdminFees.ShouldBeGreaterThan(0);
            trakteerApi.Subtotal.ShouldBeGreaterThan(0);
            trakteerApi.OrderDate.ShouldBeGreaterThan(default);
            trakteerApi.PaymentMethod.ShouldNotBeNullOrEmpty();
        }
    }

    [Theory]
    [InlineData("65190576-e653-47d2-b472-9a367a54ed23")]
    [InlineData("https://saweria.co/receipt/65190576-e653-47d2-b472-9a367a54ed23")]
    public async Task SaweriaApiTest(string url)
    {
        var saweriaApi = await mirrorPaymentRestService.GetSaweriaApi(url);

        if (saweriaApi.IsValid)
        {
            saweriaApi.OrderId.ShouldNotBeNullOrEmpty();
            saweriaApi.CendolCount.ShouldBeGreaterThan(0);
            saweriaApi.Subtotal.ShouldBeGreaterThan(0);
            saweriaApi.OrderDate.ShouldBeGreaterThan(default);
        }
    }

    [Theory()]
    [InlineData("65190576-e653-47d2-b472-9a367a54ed23")]
    [InlineData("https://saweria.co/receipt/65190576-e653-47d2-b472-9a367a54ed23")]
    public async Task SaweriaParserTest(string url)
    {
        var donationParsedDto = await mirrorPaymentRestService.ParseSaweriaWeb(url);

        if (donationParsedDto.IsValid)
        {
            donationParsedDto.IsValid.ShouldBeTrue();
            donationParsedDto.CendolCount.ShouldBeGreaterThan(0);
            donationParsedDto.OrderDate.ShouldBeGreaterThan(default);
            donationParsedDto.OrderId.ShouldNotBeNullOrEmpty();
        }
    }
}