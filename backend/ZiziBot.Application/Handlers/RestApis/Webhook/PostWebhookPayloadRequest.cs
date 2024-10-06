using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Handlers.RestApis.Webhook;

public class PostWebhookPayloadRequest : ApiRequestBase<PostWebhookPayloadResponseDto>
{
    [FromRoute(Name = "targetId")] public string targetId { get; set; }

    [FromHeader(Name = "User-Agent")] public string UserAgent { get; set; }

    [FromQuery(Name = "webhookFormat")] public WebhookFormat WebhookFormat { get; set; }

    [FromQuery(Name = "isDebug")] public bool IsDebug { get; set; }
}

public class PostWebhookPayloadResponseDto
{
    public TimeSpan Duration { get; set; }
    public object Result { get; set; }
}

public class PostWebhookPayloadHandler(
    ILogger<PostWebhookPayloadHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : IRequestHandler<PostWebhookPayloadRequest, ApiResponseBase<PostWebhookPayloadResponseDto>>
{
    public async Task<ApiResponseBase<PostWebhookPayloadResponseDto>> Handle(
        PostWebhookPayloadRequest request,
        CancellationToken cancellationToken
    )
    {
        var stopwatch = Stopwatch.StartNew();
        var webhookSource = request.UserAgent.GetWebHookSource();
        var webhookHeader = WebhookHeader.Parse(request.Headers);
        var content = await request.RequestBody();
        var response = new ApiResponseBase<PostWebhookPayloadResponseDto>() {
            TransactionId = request.HttpContextAccessor?.HttpContext?.TraceIdentifier ?? string.Empty
        };

        if (content == null)
        {
            return response.BadRequest("Webhook payload is empty");
        }

        var webhookChat = await dataFacade.ChatSetting.GetWebhookRouteById(request.targetId);

        if (webhookChat == null)
        {
            return response.BadRequest("Webhook route not found");
        }

        if (request.IsDebug)
        { }

        var webhookResponse = webhookSource switch {
            WebhookSource.GitHub => await serviceFacade.WebhookService.ParseGitHub(webhookHeader, content),
            WebhookSource.GitLab => await serviceFacade.WebhookService.ParseGitLab(webhookHeader, content),
            _ => default
        };

        if (webhookResponse == null)
        {
            return response.BadRequest("Webhook can't be processed");
        }

        await serviceFacade.MediatorService.EnqueueAsync(new SendWebhookMessageRequest() {
            targetId = request.targetId,
            Event = webhookHeader.Event,
            TransactionId = request.TransactionId,
            WebhookSource = webhookSource,
            RawHeaders = request.Headers.ToHeaderRawKv(),
            RawBody = content,
            FormattedHtml = webhookResponse.FormattedHtml
        }, ExecutionStrategy.Hangfire);

        return response.Success("Webhook payload processed", new PostWebhookPayloadResponseDto() {
            Duration = stopwatch.Elapsed
        });
    }
}