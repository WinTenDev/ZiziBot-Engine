using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Features.Handlers.RestApis.DashboardSession;

public class CheckDashboardSessionRequestDto : ApiRequestBase<CheckDashboardSessionResponseDto>
{
    public long UserId { get; set; }
    public string SessionId { get; set; }
}

public class CheckDashboardSessionResponseDto
{
    public bool IsSessionValid { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }
}

public class CheckDashboardSessionRequestHandler(
    ILogger<CheckDashboardSessionRequestHandler> logger,
    DataFacade dataFacade
)
    : IApiRequestHandler<CheckDashboardSessionRequestDto, CheckDashboardSessionResponseDto>
{
    public async Task<ApiResponseBase<CheckDashboardSessionResponseDto>> Handle(CheckDashboardSessionRequestDto request, CancellationToken cancellationToken)
    {
        ApiResponseBase<CheckDashboardSessionResponseDto> responseDto = new() {
            Result = new CheckDashboardSessionResponseDto()
        };

        #region Check Dashboard Session
        var dashboardSession = await dataFacade.MongoDb.DashboardSessions
            .Where(session => session.TelegramUserId == request.UserId)
            .Where(session => session.SessionId == request.SessionId)
            .Where(session => session.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        responseDto.Result.IsSessionValid = dashboardSession != null;

        if (dashboardSession == null)
            return responseDto;
        #endregion

        #region Get User Role
        var checkSudo = await dataFacade.MongoDb.Sudoers
            .FirstOrDefaultAsync(sudoer => sudoer.UserId == request.UserId, cancellationToken: cancellationToken);

        if (checkSudo != null)
        {
            responseDto.Result.RoleId = 1;
            responseDto.Result.RoleName = "Sudo";
        }
        #endregion

        logger.LogDebug("Session {SessionId} for user {UserId} is? {@Session}", request.SessionId, request.UserId, dashboardSession);

        return responseDto;
    }
}