namespace ZiziBot.Application.Handlers.RestApis.Note;

public class ListNoteRequest : ApiRequestBase<List<NoteDto>>
{
    public long ChatId { get; set; }
}

public class ListNoteHandler(
    DataFacade dataFacade
) : IRequestHandler<ListNoteRequest, ApiResponseBase<List<NoteDto>>>
{
    public async Task<ApiResponseBase<List<NoteDto>>> Handle(ListNoteRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<List<NoteDto>>();

        if (!request.ListChatId.Contains(request.ChatId))
        {
            return response.BadRequest("ChatId is not in your list");
        }

        var listNote = await dataFacade.ChatSetting.GetListNote(request.ChatId);

        return response.Success("Get Note successfully", listNote);
    }
}