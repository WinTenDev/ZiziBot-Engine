using Flurl;

namespace ZiziBot.Application.Common.Utils;

public static class UrlUtil
{
    public static bool IsValidUrl(this string? urlPath)
    {
        return Url.IsValid(urlPath);
    }

    public static Url UrlParse(this string url)
    {
        return Url.Parse(url);
    }

    public static string? UrlSegment(this string url, int index, string? defaultValue = null)
    {
        return Url.Parse(url).PathSegments.ElementAtOrDefault(index) ?? defaultValue;
    }

    public static string GetTrakteerUrl(this string orderId)
    {
        if (!orderId.StartsWith("https://trakteer.id/payment-status"))
        {
            orderId = Url.Combine("https://trakteer.id/payment-status", orderId);
        }

        return orderId;
    }

    public static string GetSaweriaUrl(this string orderId)
    {
        if (!orderId.StartsWith("https://saweria.co/receipt"))
        {
            orderId = Url.Combine("https://saweria.co/receipt", orderId);
        }

        return orderId;
    }
}