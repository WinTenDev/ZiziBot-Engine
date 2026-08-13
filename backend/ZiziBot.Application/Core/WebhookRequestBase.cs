namespace ZiziBot.Application.Core;

public interface IWebhookRequestBase<TResult> : IAppCommand<WebhookResponseBase<TResult>>
{
}

public class WebhookResponseBase<TResult>
{
    public string FormattedHtml { get; set; }
}
