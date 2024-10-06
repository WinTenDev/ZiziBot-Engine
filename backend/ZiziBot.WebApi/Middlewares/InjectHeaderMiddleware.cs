using System.Net;
using Microsoft.AspNetCore.Http;
using MongoFramework.Linq;

namespace ZiziBot.WebApi.Middlewares;

public class InjectHeaderMiddleware(MongoDbContextBase mongoDbContext) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var bearerToken = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(bearerToken))
        {
            await next(context);
        }

        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var ignorePaths = new[]
        {
            "/api/webhook",
            "/api/logging"
        };

        if (ignorePaths.Any(s => context.Request.Path.Value.StartsWith(s)))
        {
            await next(context);
            return;
        }

        #region Check Dashboard Session
        var dashboardSession = await mongoDbContext.DashboardSessions
            .Where(entity =>
                entity.BearerToken == bearerToken &&
                entity.Status == (int)EventStatus.Complete
            )
            .FirstOrDefaultAsync();

        if (dashboardSession == null)
        {
            await context.Response.WriteAsJsonAsync(new ApiResponseBase<object>()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized"
            });

            return;
        }

        context.Request.Headers.TryAdd(HeaderKey.UserId, dashboardSession.TelegramUserId.ToString());
        #endregion

        #region Add List ChatId
        var chatAdmin = await mongoDbContext.ChatAdmin
            .Where(entity =>
                entity.UserId == dashboardSession.TelegramUserId &&
                entity.Status == (int)EventStatus.Complete
            )
            .ToListAsync();

        var chatIds = chatAdmin.Select(y => y.ChatId).Distinct();

        context.Request.Headers.TryAdd(HeaderKey.ListChatId, chatIds.ToJson());
        #endregion

        #region Add User Role
        var userRole = ApiRole.Guest;

        var checkSudo = await mongoDbContext.Sudoers
            .FirstOrDefaultAsync(entity =>
                entity.UserId == dashboardSession.TelegramUserId &&
                entity.Status == (int)EventStatus.Complete);

        if (checkSudo != null)
        {
            userRole = ApiRole.Sudo;
        }

        context.Request.Headers.TryAdd(HeaderKey.UserRole, userRole.ToString());
        #endregion

        await next(context);
    }
}