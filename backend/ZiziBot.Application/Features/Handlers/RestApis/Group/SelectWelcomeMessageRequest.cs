using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.Handlers.RestApis.Group;

public class SelectWelcomeMessageRequest : ApiRequestBase<object>
{
    [FromBody]
    public SelectWelcomeMessageRequestModel Model { get; set; }
}

public class SelectWelcomeMessageRequestModel
{
    public long ChatId { get; set; }
    public string WelcomeId { get; set; }
}

public class SelectWelcomeMessageHandler(
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade
) : IApiRequestHandler<SelectWelcomeMessageRequest, object>
{
    public async Task<ApiResponseBase<object>> Handle(SelectWelcomeMessageRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<object>();

        var listWelcomeMessage = await dataFacade.MongoDb.WelcomeMessage
            .Where(entity => entity.ChatId == request.Model.ChatId)
            .Where(entity => entity.Status != EventStatus.Deleted)
            .Where(entity => httpContextHelper.UserInfo.ListChatId.Contains(entity.ChatId))
            .ToListAsync(cancellationToken: cancellationToken);

        if (listWelcomeMessage.Count == 0)
        {
            return response.BadRequest("Welcome Message not found", null);
        }

        listWelcomeMessage.ForEach(row => {
            row.Status = EventStatus.Inactive;
        });

        var selectedWelcome = listWelcomeMessage.FirstOrDefault(entity => entity.Id.ToString() == request.Model.WelcomeId);

        if (selectedWelcome == null)
        {
            return response.BadRequest("Welcome Message not found", null);
        }

        selectedWelcome.Status = EventStatus.Complete;

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return response.Success("Welcome Message activated successfully.", true);
    }
}