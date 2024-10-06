using Microsoft.AspNetCore.Mvc;

namespace ZiziBot.Application.Handlers.RestApis.Group;

public class GetWelcomeMessageRequest : ApiRequestBase<WelcomeMessageDto>
{
    [FromRoute] public string WelcomeId { get; set; }
}

public class GetWelcomeMessageHandler(
    DataFacade dataFacade
) : IRequestHandler<GetWelcomeMessageRequest, ApiResponseBase<WelcomeMessageDto>>
{
    public async Task<ApiResponseBase<WelcomeMessageDto>> Handle(GetWelcomeMessageRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<WelcomeMessageDto>();

        var data = await dataFacade.Group.GetWelcomeMessageById(request.WelcomeId);

        return data == null ?
            response.BadRequest("Welcome Message not found") :
            response.Success("Get Welcome Message successfully", data);
    }
}