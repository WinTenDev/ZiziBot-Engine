using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.Handlers.RestApis.Group;

public class ListWelcomeMessageRequest : ApiRequestBase<List<WelcomeMessageResponse>>
{
    [FromQuery]
    public long ChatId { get; set; }
}

public class WelcomeMessageResponse
{
    public string Id { get; set; }
    public long ChatId { get; set; }
    public string Text { get; set; }
    public string RawButton { get; set; }
    public string Media { get; set; }
    public int DataType { get; set; }
    public string DataTypeName { get; set; }
    public int Status { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string StatusName { get; set; }
}

public class ListWelcomeMessageHandler(
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade
) : IApiRequestHandler<ListWelcomeMessageRequest, List<WelcomeMessageResponse>>
{
    public async Task<ApiResponseBase<List<WelcomeMessageResponse>>> Handle(ListWelcomeMessageRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<List<WelcomeMessageResponse>>();

        var query = await dataFacade.MongoDb.WelcomeMessage.AsNoTracking()
            .WhereIf(request.ChatId != 0, entity => entity.ChatId == request.ChatId)
            .Where(entity => entity.Status != (int)EventStatus.Deleted)
            .Where(entity => httpContextHelper.UserInfo.ListChatId.Contains(entity.ChatId))
            .ToListAsync(cancellationToken: cancellationToken);

        var data = query.Select(entity => new WelcomeMessageResponse {
            Id = entity.Id.ToString(),
            ChatId = entity.ChatId,
            Text = entity.Text,
            RawButton = entity.RawButton,
            Media = entity.Media,
            DataType = (int)entity.DataType,
            DataTypeName = entity.DataType.ToString(),
            Status = (int)entity.Status,
            StatusName = entity.Status.ToString(),
            UpdatedDate = entity.UpdatedDate
        }).ToList();

        return response.Success("Get Welcome Message successfully", data);
    }
}