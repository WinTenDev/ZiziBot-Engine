using Microsoft.EntityFrameworkCore;

public class ListPendekinRequest : ApiRequestBase<List<ListPendekinResponse>>
{ }

public class ListPendekinResponse
{
    public string PendekinId { get; set; }
    public string ShortPath { get; set; }
    public string OriginalUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class ListPendekinHandler(
    DataFacade dataFacade
) : IApiRequestHandler<ListPendekinRequest, List<ListPendekinResponse>>
{
    public async Task<ApiResponseBase<List<ListPendekinResponse>>> Handle(ListPendekinRequest request, CancellationToken cancellationToken)
    {
        var listPendekin = await dataFacade.MongoEf.PendekinMap.AsNoTracking()
            .Select(x => new ListPendekinResponse() {
                PendekinId = x.Id.ToString(),
                ShortPath = x.ShortPath,
                OriginalUrl = x.OriginalUrl,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return ApiResponseBase.ReturnSuccess("Get list Pendekin successfully", listPendekin);
    }
}