using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.Handlers.RestApis.MirrorUser;

public class GetMirrorUsersRequestDto : ApiRequestBase<IEnumerable<GetMirrorUserResponseDto>>
{ }

public class GetMirrorUserResponseDto
{
    public string Id { get; set; }
    public long UserId { get; set; }
    public DateTime ExpireDate { get; set; }
    public int Status { get; set; }
    public string TransactionId { get; set; }
    public DateTime MemberSince { get; set; }
    public DateTime LastUpdate { get; set; }
}

public class GetMirrorUsersRequestHandler(
    DataFacade dataFacade
) : IApiRequestHandler<GetMirrorUsersRequestDto, IEnumerable<GetMirrorUserResponseDto>>
{
    private readonly ApiResponseBase<IEnumerable<GetMirrorUserResponseDto>> _response = new();

    public async Task<ApiResponseBase<IEnumerable<GetMirrorUserResponseDto>>> Handle(GetMirrorUsersRequestDto request, CancellationToken cancellationToken)
    {
        var user = await dataFacade.MongoDb.MirrorUser
            .Where(mirrorUser => mirrorUser.Status == EventStatus.Complete)
            .ToListAsync(cancellationToken: cancellationToken);

        var mirrorUsers = user.Select(mirrorUser => new GetMirrorUserResponseDto {
            Id = mirrorUser.Id.ToString(),
            UserId = mirrorUser.UserId,
            ExpireDate = mirrorUser.ExpireDate,
            Status = (int)mirrorUser.Status,
            TransactionId = mirrorUser.TransactionId,
            MemberSince = mirrorUser.CreatedDate,
            LastUpdate = mirrorUser.UpdatedDate
        });

        return _response.Success("Users found", mirrorUsers);
    }
}