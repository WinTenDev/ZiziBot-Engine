using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Features.Handlers.RestApis.DashboardSession;

public class CheckDashboardBearerSessionRequestDto : ApiRequestBase<CheckDashboardBearerSessionResponseDto>
{ }

public class CheckDashboardBearerSessionResponseDto
{
    public bool IsSessionValid { get; set; }
    public string UserName { get; set; }
    public string PhotoUrl { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public List<UserFeature> Features { get; set; }
}

public class UserFeature
{
    public string Title { get; set; }
    public string Url { get; set; }
    public int MinimumRole { get; set; }
}

public class CheckDashboardBearerSessionRequestHandler(
    ILogger<CheckDashboardSessionRequestHandler> logger,
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade
)
    : IApiRequestHandler<CheckDashboardBearerSessionRequestDto, CheckDashboardBearerSessionResponseDto>
{
    public async Task<ApiResponseBase<CheckDashboardBearerSessionResponseDto>> Handle(CheckDashboardBearerSessionRequestDto request, CancellationToken cancellationToken)
    {
        ApiResponseBase<CheckDashboardBearerSessionResponseDto> response = new();

        #region Check Dashboard Session
        var dashboardSession = await dataFacade.MongoDb.DashboardSessions
            .Where(entity =>
                entity.BearerToken == httpContextHelper.UserInfo.BearerToken &&
                entity.Status == EventStatus.Complete
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (dashboardSession == null)
        {
            return response.Unauthorized("Session not found");
        }

        var userId = dashboardSession.TelegramUserId;
        #endregion

        #region Get User Role
        var result = new CheckDashboardBearerSessionResponseDto() {
            IsSessionValid = true,
            UserName = dashboardSession.FirstName,
            PhotoUrl = dashboardSession.PhotoUrl,
            RoleId = 2,
            RoleName = "User",
            Features = new()
        };

        var checkSudo = await dataFacade.MongoDb.Sudoers
            .FirstOrDefaultAsync(entity =>
                    entity.UserId == userId &&
                    entity.Status == EventStatus.Complete,
                cancellationToken);

        if (checkSudo != null)
        {
            result.RoleId = 1;
            result.RoleName = "Sudo";
        }
        #endregion

        result.Features = GetFeatures(result.RoleId);

        logger.LogDebug("Session {SessionId} for user {UserId} is? {@Session}", dashboardSession.SessionId, userId, dashboardSession);

        return response.Success("Session is valid", result);
    }

    List<UserFeature> GetFeatures(int roleId)
    {
        var features = new List<UserFeature>() {
            new() {
                Title = "Mirror User",
                Url = "/mirror-user/management",
                MinimumRole = 1
            },
            new() {
                Title = "Fed Management",
                Url = "/antispam/fed-ban-management",
                MinimumRole = 1
            },
            new() {
                Title = "Notes Management",
                Url = "/notes/notes-management",
                MinimumRole = 1
            },
            new() {
                Title = "Angular",
                Url = "/angular",
                MinimumRole = 9
            }
        };

        return features
            .Where(x => x.MinimumRole >= roleId)
            .ToList();
    }
}