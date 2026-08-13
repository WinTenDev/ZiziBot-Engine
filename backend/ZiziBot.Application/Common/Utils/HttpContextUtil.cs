using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace ZiziBot.Application.Common.Utils;

public static class HttpContextUtil
{
    private static readonly ILogger Logger = Log.ForContext(typeof(HttpContextUtil));

    public static string GetTransactionId(this IHttpContextAccessor? contextAccessor)
    {
        var transactionId = contextAccessor?.HttpContext?.Request.Headers[RequestKey.TransactionId].ToString();
        return transactionId ?? string.Empty;
    }

    public static long GetUserId(this IHttpContextAccessor contextAccessor)
    {
        var userId = Convert.ToInt64(contextAccessor.HttpContext?.Request.Headers[RequestKey.UserId]);
        return userId;
    }

    public static WebhookSource GetWebHookSource(this HttpRequest httpRequest)
    {
        var headers = httpRequest.Headers;
        var source = headers switch {
            _ when headers.Any(pair => pair.Key.Contains("GitHub", StringComparison.InvariantCultureIgnoreCase)) =>
                WebhookSource.GitHub,
            _ => WebhookSource.Unknown
        };

        return source != WebhookSource.Unknown ? source : WebhookSource.Unknown;
    }

    public static WebhookSource GetWebHookSource(this string userAgent)
    {
        var source = userAgent.ToLower() switch {
            _ when userAgent.Like("GitHub") => WebhookSource.GitHub,
            _ when userAgent.Like("GitLab") => WebhookSource.GitLab,
            _ when userAgent.Like("SonarQube") => WebhookSource.SonarQube,
            _ => WebhookSource.Unknown
        };

        Logger.Verbose("GetWebHookSource. UserAgent: {UserAgent}, Source: {Source}", userAgent, source);

        return source;
    }

    public static string GetCookieValue(this IRequestCookieCollection cookieCollection, string key)
    {
        return cookieCollection.TryGetValue(key, out var value) ? value : string.Empty;
    }

    public static JwtSecurityToken? DecodeJwt(this string? bearerToken)
    {
        if (bearerToken.IsNullOrWhiteSpace())
            return default;

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(bearerToken);

        return jwtToken;
    }
}