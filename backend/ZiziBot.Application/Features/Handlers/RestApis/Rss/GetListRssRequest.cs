using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.Handlers.RestApis.Rss;

public class GetListRssRequest : ApiRequestBase<List<GetListRssResponse>>
{
    [FromQuery]
    public long ChatId { get; set; }
}

public class GetListRssResponse
{
    public string Id { get; set; }
    public string Url { get; set; }
    public long ChatId { get; set; }
    public string LastErrorMessage { get; set; }
    public string CronJobId { get; set; }
    public string TransactionId { get; set; }
    public int Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class GetListRssHandler(
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade
) : IApiRequestHandler<GetListRssRequest, List<GetListRssResponse>>
{
    public async Task<ApiResponseBase<List<GetListRssResponse>>> Handle(GetListRssRequest request, CancellationToken cancellationToken)
    {
        ApiResponseBase<List<GetListRssResponse>> response = new();

        var listRss = await dataFacade.MongoDb.RssSetting
            .WhereIf(request.ChatId != 0, entity => entity.ChatId == request.ChatId)
            .Where(entity => httpContextHelper.UserInfo.ListChatId.Contains(entity.ChatId))
            .Where(entity => entity.Status == EventStatus.Complete)
            .ToListAsync(cancellationToken: cancellationToken);

        var result = listRss.Select(x => new GetListRssResponse {
            Id = x.Id.ToString(),
            Url = x.RssUrl,
            ChatId = x.ChatId,
            LastErrorMessage = x.LastErrorMessage,
            CronJobId = x.CronJobId,
            Status = (int)x.Status,
            TransactionId = x.TransactionId,
            CreatedDate = x.CreatedDate,
            UpdatedDate = x.UpdatedDate
        }).ToList();

        return response.Success("Get list RSS success", result);
    }
}