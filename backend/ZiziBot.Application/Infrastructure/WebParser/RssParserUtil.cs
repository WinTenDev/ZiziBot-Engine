using CodeHollow.FeedReader;
using Flurl;
using Octokit;
using Serilog;
using ZiziBot.Application.Common.Constants;
using ZiziBot.Application.Common.Utils;
using Feed = CodeHollow.FeedReader.Feed;

namespace ZiziBot.Application.Infrastructure.WebParser;

public static class RssParserUtil
{
    private static readonly ILogger Logger = LoggerUtil.CreateLogger(typeof(RssParserUtil));

    public static async Task<Feed> ReadRssAsync(this string rssUrl, bool throwIfError = false)
    {
        try
        {
            var feed = await FeedReader.ReadAsync(rssUrl, userAgent: Env.COMMON_UA);
            return feed;
        }
        catch (Exception e)
        {
            if (throwIfError)
                throw;

            Logger.Error(e, "Error reading rss: {RssUrl}. Message: {Message}", rssUrl, e.Message);

            return new Feed();
        }
    }

    public static async Task<string> DetectRss(this string rssUrl, int attempt = 1, bool throwIfError = false)
    {
        try
        {
            var fixedUrl = rssUrl.TrimEnd("/");

            while (true)
            {
                const int maxAttempt = 3;

                if (attempt > maxAttempt)
                    throw new Exception($"Unable to detect rss Url: {rssUrl}");

                var readRss = await rssUrl.ReadRssAsync();
                if (!readRss.Items.IsEmpty())
                    return fixedUrl;

                if ((rssUrl.IsGithubReleaseUrl() || rssUrl.IsGithubCommitsUrl()) && !fixedUrl.EndsWith(".atom"))
                {
                    fixedUrl = rssUrl.AppendPathSegment(".atom");
                    var read = await fixedUrl.ReadRssAsync();
                    if (!read.Items.IsEmpty())
                        return fixedUrl;
                }

                if (!rssUrl.EndsWith("/feed"))
                {
                    fixedUrl = rssUrl.AppendPathSegment("feed");
                    var read = await fixedUrl.ReadRssAsync();
                    if (!read.Items.IsEmpty())
                        return fixedUrl;
                }

                if (!rssUrl.EndsWith("/rss"))
                {
                    fixedUrl = rssUrl.AppendPathSegment("rss");
                    var read = await fixedUrl.ReadRssAsync();
                    if (!read.Items.IsEmpty())
                        return fixedUrl;
                }

                var urlParse = rssUrl.UrlParse();
                var urlParseScheme = urlParse.Scheme + "://" + urlParse.Host;

                fixedUrl = urlParseScheme;
                attempt += 1;
            }
        }
        catch (Exception e)
        {
            Logger.Warning(e, "Unable detect RSS from: {RssUrl}. Message: {Message}", rssUrl, e.Message);

            if (throwIfError)
                throw;

            return rssUrl;
        }
    }

    public static bool IsGithubReleaseUrl(this string url)
    {
        return url.Contains("github.com") && url.Contains("releases");
    }

    public static bool IsGithubCommitsUrl(this string url)
    {
        return url.Contains("github.com") && url.Contains("commits");
    }

    public static bool IsGithubUrl(this string url)
    {
        return url.Contains("github.com");
    }

    public static async Task<Release?> GetGithubAssetLatest(this string url, string? token = null)
    {
        if (!url.IsGithubReleaseUrl())
            return null;

        Logger.Debug("Collecting GitHub assets latest for URL: {Url}", url);
        var client = new GitHubClient(new ProductHeaderValue("ZiziBot"));

        if (!string.IsNullOrWhiteSpace(token))
            client.Credentials = new Credentials(token);

        var repoGroup = url.Split("/")[3];
        var repoName = url.Split("/")[4];

        Logger.Debug("Getting GitHub release assets latest for URL: {Url}", url);
        var release = await client.Repository.Release.GetLatest(repoGroup, repoName);

        Logger.Debug("Got Assets for URl: {Url} {Count} item(s)", url, release.Assets.Count);
        return release;
    }

    public static async Task<IReadOnlyList<Release>?> GetGithubAssets(this string url, string? token = null)
    {
        if (!url.IsGithubReleaseUrl())
            return null;

        Logger.Debug("Collecting GitHub assets for URL: {Url}", url);
        var client = new GitHubClient(new ProductHeaderValue("ZiziBot"));

        if (!string.IsNullOrWhiteSpace(token))
            client.Credentials = new Credentials(token);

        var repoGroup = url.Split("/")[3];
        var repoName = url.Split("/")[4];

        Logger.Debug("Getting GitHub release assets for URL: {Url}", url);
        var release = await client.Repository.Release.GetAll(repoGroup, repoName);

        Logger.Debug("Got Assets for URl: {Url} {Count} item(s)", url, release.Count);
        return release;
    }
}