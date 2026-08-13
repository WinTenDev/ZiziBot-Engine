namespace ZiziBot.Application.Features.Handlers.RestApis.Samples;

public class EnqueueOutboxSampleRequest : ApiPostRequestBase<EnqueueOutboxSampleBody, EnqueueOutboxSampleResponse>
{
}

public class EnqueueOutboxSampleBody
{
    public string Message { get; set; }
}

public class EnqueueOutboxSampleResponse
{
    public string TransactionId { get; set; }
}

public class EnqueueOutboxSampleHandler(
    IHttpContextHelper httpContextHelper,
    OutboxService outboxService
) : IApiRequestHandler<EnqueueOutboxSampleRequest, EnqueueOutboxSampleResponse>
{
    private readonly ApiResponseBase<EnqueueOutboxSampleResponse> response = new();

    public async Task<ApiResponseBase<EnqueueOutboxSampleResponse>> Handle(EnqueueOutboxSampleRequest request, CancellationToken cancellationToken)
    {
        var transactionId = httpContextHelper.UserInfo.TransactionId;

        await outboxService.EnqueueAndSaveAsync(
            type: "sample.outbox",
            payload: new { request.Body.Message },
            transactionId: transactionId,
            cancellationToken: cancellationToken
        );

        return response.Success("Outbox message enqueued", new EnqueueOutboxSampleResponse
        {
            TransactionId = transactionId
        });
    }
}
