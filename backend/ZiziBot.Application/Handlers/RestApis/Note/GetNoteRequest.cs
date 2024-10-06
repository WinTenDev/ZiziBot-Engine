using Microsoft.AspNetCore.Mvc;

namespace ZiziBot.Application.Handlers.RestApis.Note;

public class GetNoteRequest : ApiRequestBase<object>
{
    [FromRoute] public string NoteId { get; set; }
}

public class GetNoteHandler(
    DataFacade dataFacade
) : IRequestHandler<GetNoteRequest, ApiResponseBase<object>>
{
    public async Task<ApiResponseBase<object>> Handle(GetNoteRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<object>();

        var notes = await dataFacade.ChatSetting.GetNote(request.NoteId);

        return response.Success("Get detail Note successfully", notes);
    }
}