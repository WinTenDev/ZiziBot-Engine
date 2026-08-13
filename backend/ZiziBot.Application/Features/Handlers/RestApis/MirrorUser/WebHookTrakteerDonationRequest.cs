using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.RestApis.MirrorUser;

public class WebHookTrakteerDonationRequest : ApiPostRequestBase<WebHookTrakteerDonationRequestBody, WebHookTrakteerDonationResponse>
{ }

public class WebHookTrakteerDonationRequestBody
{
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("supporter_name")]
    public string SupporterName { get; set; }

    [JsonPropertyName("supporter_avatar")]
    public string SupporterAvatar { get; set; }

    [JsonPropertyName("supporter_message")]
    public string SupporterMessage { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; }

    [JsonPropertyName("unit_icon")]
    public string UnitIcon { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("net_amount")]
    public int NetAmount { get; set; }
}

public class WebHookTrakteerDonationRequestValidator : AbstractValidator<WebHookTrakteerDonationRequestBody>
{ }

public class WebHookTrakteerDonationResponse
{ }

public class WebHookTrakteerDonationHandler(
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade,
    OutboxService outboxService
) : IApiRequestHandler<WebHookTrakteerDonationRequest, WebHookTrakteerDonationResponse>
{
    private readonly ApiResponseBase<WebHookTrakteerDonationResponse> response = new();

    public async Task<ApiResponseBase<WebHookTrakteerDonationResponse>> Handle(WebHookTrakteerDonationRequest request, CancellationToken cancellationToken)
    {
        var mirrorConfig = await dataFacade.AppSetting.GetRequiredConfigSectionAsync<MirrorConfig>();
        var webHookToken = mirrorConfig.TrakteerWebHookToken;

        if (webHookToken.IsNotNullOrWhiteSpace())
        {
            if (!httpContextHelper.HeaderDict.TryGetValue("X-Webhook-Token", out var headerToken))
            {
                return response.Unauthorized("Token harus diisi pada header X-Webhook-Token");
            }

            if (headerToken != webHookToken)
            {
                return response.Unauthorized("Token tidak valid, silakan atur Token di Setting");
            }
        }

        var mirrorDonation = await dataFacade.MongoDb.MirrorDonation
            .Where(x => x.OrderId == request.Body.TransactionId)
            .Where(x => x.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (mirrorDonation != null)
        {
            return response.Success("Donasi Mirror sudah diproses");
        }

        dataFacade.MongoDb.MirrorDonation.Add(new MirrorDonationEntity
        {
            Status = EventStatus.Complete,
            TransactionId = httpContextHelper.UserInfo.TransactionId,
            OrderId = request.Body.TransactionId,
            OrderDate = request.Body.CreatedAt,
            Type = request.Body.Type,
            SupporteName = request.Body.SupporterName,
            SupporterAvatar = request.Body.SupporterAvatar,
            SupporterMessage = request.Body.SupporterMessage,
            Unit = request.Body.Unit,
            UnitIcon = request.Body.UnitIcon,
            Quantity = request.Body.Quantity,
            Price = request.Body.Price,
            NetAmount = request.Body.NetAmount,
            Source = DonationSource.Trakteer
        });

        await outboxService.EnqueueAsync(
            type: "mirror-donation.webhook.trakteer",
            payload: new
            {
                request.Body.TransactionId,
                request.Body.CreatedAt,
                request.Body.Type,
                request.Body.SupporterName,
                request.Body.Quantity,
                request.Body.Price,
                request.Body.NetAmount
            },
            transactionId: httpContextHelper.UserInfo.TransactionId,
            cancellationToken: cancellationToken
        );

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return response.Success("Trakteer webhook berhasil diproses");
    }
}
