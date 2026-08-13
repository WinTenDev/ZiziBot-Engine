using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.RestApis.MirrorUser;

public class PostMirrorUserRequestDto : ApiRequestBase<bool>
{
    public long UserId { get; set; }
    public int AddDays { get; set; }
    public int MonthDuration { get; set; }
    public string? AdditionalNote { get; set; }
}

public class PostMirrorUserRequestHandler(
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade,
    OutboxService outboxService
) : IApiRequestHandler<PostMirrorUserRequestDto, bool>
{
    private readonly ApiResponseBase<bool> _response = new();

    public async Task<ApiResponseBase<bool>> Handle(PostMirrorUserRequestDto request, CancellationToken cancellationToken)
    {
        var mirrorUser = await dataFacade.MongoDb.MirrorUser
            .Where(entity => entity.UserId == request.UserId)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (mirrorUser == null)
        {
            var expireDate = DateTime.UtcNow.AddDays(request.AddDays);

            dataFacade.MongoDb.MirrorUser.Add(new MirrorUserEntity()
            {
                UserId = request.UserId,
                ExpireDate = expireDate,
                Status = EventStatus.Complete,
                TransactionId = httpContextHelper.UserInfo.TransactionId
            });

            await outboxService.EnqueueAsync(
                type: "mirror-user.created",
                payload: new
                {
                    request.UserId,
                    ExpireDate = expireDate,
                    request.AddDays,
                    request.MonthDuration,
                    request.AdditionalNote,
                    ActorUserId = httpContextHelper.UserInfo.UserId
                },
                transactionId: httpContextHelper.UserInfo.TransactionId,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            mirrorUser.ExpireDate = mirrorUser.ExpireDate.AddMonths(request.MonthDuration);
            mirrorUser.TransactionId = httpContextHelper.UserInfo.TransactionId;

            await outboxService.EnqueueAsync(
                type: "mirror-user.extended",
                payload: new
                {
                    request.UserId,
                    mirrorUser.ExpireDate,
                    request.MonthDuration,
                    request.AdditionalNote,
                    ActorUserId = httpContextHelper.UserInfo.UserId
                },
                transactionId: httpContextHelper.UserInfo.TransactionId,
                cancellationToken: cancellationToken
            );
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return _response.Success("Mirror User saved", true);
    }
}
