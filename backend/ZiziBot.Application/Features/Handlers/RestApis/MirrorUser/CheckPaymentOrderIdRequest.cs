using ZiziBot.Application.Features.UseCases.Mirror;
using ZiziBot.Application.Common.Dtos;

namespace ZiziBot.Application.Features.Handlers.RestApis.MirrorUser;

public class CheckPaymentOrderIdRequest : ApiRequestBase<ParsedDonationDto>
{
    public string OrderId { get; set; }
}

public class CheckPaymentOrderIdHandler(
    ServiceFacade serviceFacade,
    DonationSettlementUseCase donationSettlementUseCase
) : IApiRequestHandler<CheckPaymentOrderIdRequest, ParsedDonationDto>
{
    private readonly ApiResponseBase<ParsedDonationDto> _response = new();

    public async Task<ApiResponseBase<ParsedDonationDto>> Handle(CheckPaymentOrderIdRequest request, CancellationToken cancellationToken)
    {
        var parsedDonationDto = await donationSettlementUseCase.Handle(new DonationSettlementRequest() {
            OrderId = request.OrderId,
        });

        var parsedDonationResponse = new ParsedDonationDto() {
            OrderId = parsedDonationDto.OrderId,
            Source = parsedDonationDto.Source,
            Subtotal = parsedDonationDto.Subtotal,
            Cendols = parsedDonationDto.Cendols,
            AdminFees = parsedDonationDto.AdminFees,
            PaymentMethod = parsedDonationDto.PaymentMethod,
            OrderDate = parsedDonationDto.OrderDate,
            RawText = parsedDonationDto.RawText,
            PaymentUrl = parsedDonationDto.PaymentUrl,
        };

        return parsedDonationDto.IsValid ?
            _response.Success("Get OrderId succeed", parsedDonationResponse) :
            _response.BadRequest("OrderId not found");
    }
}