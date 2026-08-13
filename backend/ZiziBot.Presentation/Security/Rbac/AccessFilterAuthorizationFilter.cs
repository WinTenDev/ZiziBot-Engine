using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Common.Dtos;
using ZiziBot.Application.Infrastructure.Database.Service;

namespace ZiziBot.Presentation.Security.Rbac;

public class AccessFilterAuthorizationFilter(
    string flag,
    RoleLevel roleLevel,
    ILogger<AccessFilterAuthorizationFilter> logger,
    DataFacade dataFacade
) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userInfo = new UserInfo();
        var userRoles = new List<RoleLevel>();
        var transactionId = context.HttpContext.GetTransactionId();

        var response = new ApiResponseBase<object> {
            TransactionId = transactionId
        };

        logger.LogDebug("Access '{Flag}' role level minimum: {Role}", flag, roleLevel);

        if (roleLevel == RoleLevel.None) // allow anonymous
            return;

        var bearerToken = context.HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "").Trim();

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            #region Check Dashboard Session
            var dashboardSession = await dataFacade.MongoDb.DashboardSessions
                .Where(x => x.BearerToken == bearerToken)
                .Where(x => x.Status == EventStatus.Complete)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();

            if (dashboardSession == null)
            {
                response.Unauthorized("Access token is not valid");
                context.Result = new UnauthorizedObjectResult(response);
                return;
            }

            userInfo.IsAuthenticated = true;
            userInfo.BearerToken = bearerToken;
            userInfo.TransactionId = transactionId;
            userInfo.UserId = dashboardSession.TelegramUserId;
            userInfo.UserName = dashboardSession.Username;
            userInfo.UserPhotoUrl = dashboardSession.PhotoUrl;
            userInfo.UserFirstName = dashboardSession.FirstName;
            userInfo.UserLastName = dashboardSession.LastName;

            userRoles.Add(RoleLevel.User);

            dashboardSession.ExpireDate = DateTime.UtcNow.AddDays(7);
            dashboardSession.TransactionId = transactionId;
            #endregion

            #region Add User Role
            var checkSudo = await dataFacade.MongoDb.Sudoers.AsNoTracking()
                .Where(x => x.Status == EventStatus.Complete)
                .Where(x => x.UserId == dashboardSession.TelegramUserId)
                .FirstOrDefaultAsync();

            if (checkSudo != null)
            {
                userRoles.Add(RoleLevel.Sudo);
            }

            userInfo.UserRoles = userRoles;

            await dataFacade.SaveChangesAsync();
            #endregion
        }

        if (!userRoles.Contains(roleLevel))
        {
            response.Unauthorized("You are not authorized to access this resource");
            context.Result = new UnauthorizedObjectResult(response);
        }

        context.HttpContext.Items.TryAdd(ValueConst.USER_INFO, userInfo);
    }
}
