using Xunit;
using ZiziBot.Application.Features.UseCases.Mirror;
using ZiziBot.Application.Common.Enums;

namespace ZiziBot.Tests.UseCase;

public class DonationSettlementTests(DonationSettlementUseCase donationSettlementUseCase)
{
    [Theory]
    [InlineData("39dfd172-bd31-5141-9e65-c63c58ccee1b")]
    public async Task DonationSettlementTest(string orderId)
    {
        var donationSettlementResponse = await donationSettlementUseCase.Handle(new DonationSettlementRequest() {
            OrderId = orderId
        });

        donationSettlementResponse.ShouldBeOfType<DonationSettlementResponse>();
        donationSettlementResponse.OrderId.ShouldNotBeNullOrWhiteSpace();
        donationSettlementResponse.OrderId.ShouldBe(orderId);
        donationSettlementResponse.CendolCount.ShouldBeGreaterThan(0);
        donationSettlementResponse.Subtotal.ShouldBeGreaterThan(0);
        donationSettlementResponse.Source.ShouldNotBe(DonationSource.Unknown);
    }
}