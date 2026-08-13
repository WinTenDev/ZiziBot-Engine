using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.MongoDb;

namespace ZiziBot.Application.Features.Hangfire;

public class HangfireAuthorizationFilter(
    ILogger<HangfireAuthorizationFilter> logger,
    MongoDbContext mongoDbContext
) : IDashboardAsyncAuthorizationFilter
{
    public async Task<bool> AuthorizeAsync(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        httpContext.Request.Cookies.TryGetValue("bearer_token", out var accessToken);

        logger.LogDebug("Checking Hangfire authorization");

        if (accessToken != null)
        {
            var checkSession = await CheckSession(accessToken);
            logger.LogDebug("SessionId is having access? {CheckSession}", checkSession);

            if (checkSession)
                return true;
        }

        logger.LogInformation("Session expired or invalid. Redirecting Home");
        httpContext.Response.Redirect("/", true);

        return true;
    }

    private async Task<bool> CheckSession(string sessionId)
    {
        var dashboardSessions = await mongoDbContext.DashboardSessions.AsNoTracking()
            .Where(entity => entity.BearerToken == sessionId)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync();

        if (dashboardSessions == null)
            return false;

        var sudoer = await mongoDbContext.Sudoers.AsNoTracking()
            .Where(entity => entity.UserId == dashboardSessions.TelegramUserId)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync();

        return sudoer != null;
    }
}
