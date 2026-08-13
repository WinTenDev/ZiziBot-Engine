using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.Handlers.RestApis.MirrorUser;

public class DeleteMirrorUserRequestDto : ApiRequestBase<bool>
{
    public long UserId { get; set; }
}

public class DeleteMirrorUserRequestHandler(
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade,
    OutboxService outboxService
) : IApiRequestHandler<DeleteMirrorUserRequestDto, bool>
{
    private readonly ApiResponseBase<bool> _response = new();

    public async Task<ApiResponseBase<bool>> Handle(DeleteMirrorUserRequestDto request, CancellationToken cancellationToken)
    {
        var mirrorUser = await dataFacade.MongoDb.MirrorUser
            .Where(x => x.Status == EventStatus.Complete)
            .Where(x => x.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (mirrorUser == null)
        {
            return _response.BadRequest("Mirror User not found");
        }

        mirrorUser.Status = EventStatus.Deleted;
        mirrorUser.TransactionId = httpContextHelper.UserInfo.TransactionId;

        await outboxService.EnqueueAsync(
            type: "mirror-user.deleted",
            payload: new
            {
                request.UserId,
                ActorUserId = httpContextHelper.UserInfo.UserId
            },
            transactionId: httpContextHelper.UserInfo.TransactionId,
            cancellationToken: cancellationToken
        );

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return _response.Success("Mirror User deleted", true);
    }
}
