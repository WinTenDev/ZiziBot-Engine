using System.Net;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Octokit;

namespace ZiziBot.Application.Features.UseCases.GitHub;

public class UpdateStatisticUseCase(
    ILogger<UpdateStatisticUseCase> logger,
    DataFacade dataFacade
)
{
    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 2)]
    [Queue(CronJobKey.Queue_Data)]
    public async Task Handle()
    {
        logger.LogDebug("Updating GitHub token usage statistic");

        var githubApiKey = await dataFacade.MongoDb.ApiKey
            .OrderByDescending(x => x.Remaining)
            .Where(x => x.Name == ApiKeyVendor.GitHub)
            .Where(x => x.Status != EventStatus.Inactive)
            .ToListAsync();

        var client = new GitHubClient(new ProductHeaderValue("ZiziBot"));

        foreach (var apiKey in githubApiKey)
        {
            try
            {
                var token = apiKey.ApiKey;

                if (!string.IsNullOrWhiteSpace(token))
                    client.Credentials = new Credentials(token);

                var limit = await client.RateLimit.GetRateLimits();
                var user = await client.User.Current();

                apiKey.LastUsedDate = DateTime.UtcNow;
                apiKey.Remaining = limit.Rate.Remaining;
                apiKey.Limit = limit.Rate.Limit;
                apiKey.LimitUnit = "Hourly";
                apiKey.ResetUsageDate = limit.Rate.Reset.UtcDateTime;
                apiKey.Owner = user.Login;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error update GitHub token usage statistic");

                if (e is AuthorizationException authorizationException)
                {
                    if (authorizationException.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        apiKey.Status = EventStatus.Inactive;
                        apiKey.Note = authorizationException.Message;
                    }
                }
            }
        }

        logger.LogDebug("Updating next GitHub api key");
        Env.GithubToken = githubApiKey.FirstOrDefault()?.ApiKey;

        await dataFacade.SaveChangesAsync();
    }
}