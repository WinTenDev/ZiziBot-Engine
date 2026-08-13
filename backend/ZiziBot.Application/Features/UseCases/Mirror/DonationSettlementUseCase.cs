using FluentValidation;
using Flurl;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Common.Dtos;

namespace ZiziBot.Application.Features.UseCases.Mirror;

public struct DonationSettlementRequest
{
    public string OrderId { get; set; }
}

public class DonationSettlementValidator : AbstractValidator<DonationSettlementRequest>
{
    public DonationSettlementValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}

public class DonationSettlementResponse
{
    public ParseMethod Method { get; set; }
    public bool IsValid { get; set; }
    public DonationSource Source { get; set; }
    public string? OrderId { get; set; }
    public string PaymentUrl { get; set; }
    public DateTime OrderDate { get; set; }
    public string? PaymentMethod { get; set; }
    public int CendolCount => Subtotal / ValueConst.CENDOL_PRICE;
    public string? Cendols { get; set; }
    public int AdminFees { get; set; }
    public int Subtotal { get; set; }
    public string? RawText { get; set; }
}

public class DonationSettlementUseCase(
    ILogger<DonationSettlementUseCase> logger,
    DonationSettlementValidator validator,
    MirrorPaymentRestService mirrorPaymentRestService,
    MirrorUserRepository mirrorUserRepository
)
{
    public async Task<DonationSettlementResponse> Handle(DonationSettlementRequest request)
    {
        logger.LogInformation("Reading information for OrderId: {OrderId}", request.OrderId);

        using var cts = new CancellationTokenSource();

        var donationFromDbTask = GetDonationFromDb(request.OrderId);
        var trakteerApiTask = mirrorPaymentRestService.GetTrakteerApi(request.OrderId, cts.Token);
        var trakteerWebTask = mirrorPaymentRestService.ParseTrakteerWeb(request.OrderId, cts.Token);

        var saweriaApiTask = mirrorPaymentRestService.GetSaweriaApi(request.OrderId, cts.Token);
        var saweriaWebTask = mirrorPaymentRestService.ParseSaweriaWeb(request.OrderId, cts.Token);

        var parsedDonationTask = await Task.WhenAny(donationFromDbTask, trakteerApiTask, trakteerWebTask, saweriaApiTask, saweriaWebTask);
        var parsedDonationDto = await parsedDonationTask;

        logger.LogDebug("OrderId: {OrderId} from Source: {Source} is Valid: {IsValid}", request.OrderId, parsedDonationDto.Source, parsedDonationDto.IsValid);
        await cts.CancelAsync();

        var donationSettlement = new DonationSettlementResponse()
        {
            Method = parsedDonationDto.Method,
            IsValid = parsedDonationDto.IsValid,
            Source = parsedDonationDto.Source,
            OrderId = parsedDonationDto.OrderId,
            PaymentUrl = parsedDonationDto.PaymentUrl,
            OrderDate = parsedDonationDto.OrderDate,
            PaymentMethod = parsedDonationDto.PaymentMethod,
            Cendols = parsedDonationDto.Cendols,
            Subtotal = parsedDonationDto.Subtotal,
            AdminFees = parsedDonationDto.AdminFees,
            RawText = parsedDonationDto.RawText
        };

        return donationSettlement;
    }

    private async Task<ParsedDonationDto> GetDonationFromDb(string orderId)
    {
        var o = new ParsedDonationDto();
        var data = await mirrorUserRepository.GetDonation(orderId);

        if (data == null)
        {
            await Task.Delay(30000); // buffer 30 seconds
            return o;
        }

        o.Method = ParseMethod.WebHookTrakteer;
        o.IsValid = true;
        o.Source = DonationSource.Trakteer;
        o.OrderId = data.OrderId;
        o.PaymentUrl = ValueConst.TRAKTEER_PAYMENT.AppendPathSegment(data.OrderId);
        o.OrderDate = data.OrderDate;
        o.PaymentMethod = string.Empty;
        o.Cendols = data.Quantity.ToString();
        o.AdminFees = data.Price - data.NetAmount;
        o.Subtotal = data.Price;
        o.RawText = null;

        return o;
    }
}