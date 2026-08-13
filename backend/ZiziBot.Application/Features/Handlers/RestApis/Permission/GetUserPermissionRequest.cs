namespace ZiziBot.Application.Features.Handlers.RestApis.Permission;

public class GetUserPermissionRequest : ApiRequestBase<GetUserPermissionResponse>
{ }

public class GetUserPermissionResponse
{
    public long UserId { get; set; }
    public List<UserPermission> UserPermissions { get; set; }
}

public class UserPermission
{
    public long ChatId { get; set; }
    public string ChatTitle { get; set; }
}

public class GetUserPermissionHandler : IApiRequestHandler<GetUserPermissionRequest, GetUserPermissionResponse>
{
    public Task<ApiResponseBase<GetUserPermissionResponse>> Handle(GetUserPermissionRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}