using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Features.UseCases.WebHook;

public class ParseSonarQubePayloadRequest
{
    public WebhookHeader? WebhookHeader { get; set; }
    public string Json { get; set; }
}

public class ParseSonarQubePayloadUseCase
{
    public async Task<WebhookResponseBase<bool>> Handle(ParseSonarQubePayloadRequest request)
    {
        await Task.Delay(1);

        return new WebhookResponseBase<bool>() {
        };
    }
}