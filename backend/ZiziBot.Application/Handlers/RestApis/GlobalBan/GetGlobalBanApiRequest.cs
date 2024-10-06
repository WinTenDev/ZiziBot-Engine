using MongoFramework.Linq;

namespace ZiziBot.Application.Handlers.RestApis.GlobalBan;

public class GetGlobalBanApiRequest : ApiRequestBase<List<GetGlobalBanApiResponse>>
{ }

public class GetGlobalBanApiResponse
{
    public long UserId { get; set; }
    public long ChatId { get; set; }
    public string Reason { get; set; }
    public int Status { get; set; }
    public string TransactionId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class GetGlobalBanApiHandler(
    DataFacade dataFacade
) : IRequestHandler<GetGlobalBanApiRequest, ApiResponseBase<List<GetGlobalBanApiResponse>>>
{
    public async Task<ApiResponseBase<List<GetGlobalBanApiResponse>>> Handle(GetGlobalBanApiRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<List<GetGlobalBanApiResponse>>();

        var globalBanEntities = await dataFacade.MongoDb.GlobalBan.ToListAsync(cancellationToken: cancellationToken);

        var listGlobalBan = globalBanEntities.Select(entity => new GetGlobalBanApiResponse() {
            UserId = entity.UserId,
            ChatId = entity.ChatId,
            Reason = entity.Reason,
            Status = entity.Status,
            TransactionId = entity.TransactionId,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate
        }).ToList();

        return response.Success("Global ban list fetched successfully", listGlobalBan);
    }
}