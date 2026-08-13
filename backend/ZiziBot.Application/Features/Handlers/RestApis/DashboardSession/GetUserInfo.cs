namespace ZiziBot.Application.Features.Handlers.RestApis.DashboardSession;

public class GetUserInfoRequest : ApiRequestBase<GetUserInfoResponse>
{ }

public class GetUserInfoResponse
{
    public bool IsSessionValid { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public long UserId { get; set; }
    public string? PhotoUrl { get; set; }
    public List<RoleLevel> Roles { get; set; }
}

public class GetUserInfoHandler(
    DataFacade dataFacade,
    IHttpContextHelper httpContextHelper
) : IApiRequestHandler<GetUserInfoRequest, GetUserInfoResponse>
{
    public async Task<ApiResponseBase<GetUserInfoResponse>> Handle(GetUserInfoRequest request, CancellationToken cancellationToken)
    {
        var response = ApiResponse.Create<GetUserInfoResponse>();

        await Task.Delay(1, cancellationToken);

        var userInfo = httpContextHelper.UserInfo;

        if (!userInfo.IsAuthenticated)
            return response.BadRequest("Session is not valid");

        var getUserInfoResponse = new GetUserInfoResponse() {
            IsSessionValid = userInfo.IsAuthenticated,
            UserName = userInfo.UserName,
            UserId = userInfo.UserId,
            FirstName = userInfo.UserFirstName,
            LastName = userInfo.UserLastName,
            PhotoUrl = userInfo.UserPhotoUrl,
            Roles = userInfo.UserRoles
        };

        return response.Success("Get User Info success", getUserInfoResponse);
    }
}