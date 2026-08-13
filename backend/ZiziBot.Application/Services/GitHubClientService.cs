using Flurl.Http;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Vendor.GitHub;

namespace ZiziBot.Application.Services;

public class GitHubClientService(
    ILogger<GitHubClientService> logger,
    BotRepository botRepository
)
{
    public async Task<List<Release>?> GetReleaseAssets(string url)
    {
        logger.LogDebug("Fetching GitHub release assets from URL: {Url}", url);

        var uri = new Uri(url);
        var owner = uri.Segments[1].TrimEnd('/');
        var repo = uri.Segments[2].TrimEnd('/');

        var client = await WithGithubClient();
        var jsonString = await client.AppendPathSegment($"/repos/{owner}/{repo}/releases")
            .SetQueryParam("per_page", "3")
            .GetStringAsync();

        var releases = jsonString.Deserialize<List<Release>>();

        logger.LogInformation("Fetched {Count} release assets from {Owner}/{Repo}", releases.Count, owner, repo);
        return releases;
    }

    private async Task<IFlurlRequest> WithGithubClient()
    {
        var githubToken = Env.GithubToken ?? await botRepository.GetApiKeyAsync(ApiKeyCategory.Internal, ApiKeyVendor.GitHub);
        return UrlConst.GITHUB_API.WithOAuthBearerToken(githubToken);
    }
}