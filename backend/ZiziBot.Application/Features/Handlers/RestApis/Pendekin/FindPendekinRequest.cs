using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.Handlers.RestApis.Pendekin;

public class ListPendekinRequest : ApiRequestBase<List<ListPendekinResponse>>
{ }

public class ListPendekinResponse
{
    public string PendekinId { get; set; }
    public string ShortPath { get; set; }
    public string ShortUrl { get; set; }
    public string OriginalUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class ListPendekinHandler(
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade
) : IApiRequestHandler<ListPendekinRequest, List<ListPendekinResponse>>
{
    public async Task<ApiResponseBase<List<ListPendekinResponse>>> Handle(ListPendekinRequest request, CancellationToken cancellationToken)
    {
        var response = ApiResponse.Create<List<ListPendekinResponse>>();
        var pendekinConfig = await dataFacade.AppSetting.GetConfigSectionAsync<PendekinConfig>();
        if (pendekinConfig == null)
            return response.BadRequest("Pendekin not yet prepared");

        var listPendekin = await dataFacade.MongoDb.PendekinMap.AsNoTracking()
            .Where(x => x.Status == EventStatus.Complete)
            .Where(x => x.CreatedBy == httpContextHelper.UserInfo.UserId)
            .Select(x => new ListPendekinResponse() {
                PendekinId = x.Id.ToString(),
                ShortPath = x.ShortPath,
                OriginalUrl = x.OriginalUrl,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate
            })
            .OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken: cancellationToken);

        listPendekin.ForEach(x => {
            x.ShortUrl = !string.IsNullOrWhiteSpace(pendekinConfig.RouterBaseUrl) ? $"{pendekinConfig.RouterBaseUrl}/{x.ShortPath}" : "";
        });

        return response.Success("Get list Pendekin successfully", listPendekin);
    }
}