namespace ZiziBot.Application.Infrastructure.Middleware;

public class FlurlCachingMiddleware : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return base.SendAsync(request, cancellationToken);
    }
}