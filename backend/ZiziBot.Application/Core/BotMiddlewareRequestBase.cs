namespace ZiziBot.Application.Core;

public class BotMiddlewareRequestBase<T> : IAppCommand<BotMiddlewareResponseBase<T>>
{
    public long UserId { get; set; }
    public long ChatId { get; set; }
}
