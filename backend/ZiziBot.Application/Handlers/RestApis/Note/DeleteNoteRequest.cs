using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoFramework.Linq;

namespace ZiziBot.Application.Handlers.RestApis.Note;

public class DeleteNoteRequest : ApiRequestBase<bool>
{
    [FromBody] public DeleteNoteRequestBody Body { get; set; }
}

public class DeleteNoteRequestBody
{
    public long ChatId { get; set; }
    public string Id { get; set; }
}

public class DeleteNoteHandler(
    DataFacade dataFacade
) : IRequestHandler<DeleteNoteRequest, ApiResponseBase<bool>>
{
    public async Task<ApiResponseBase<bool>> Handle(DeleteNoteRequest request, CancellationToken cancellationToken)
    {
        ApiResponseBase<bool> response = new();

        var note = await dataFacade.MongoDb.Note
            .Where(entity => entity.ChatId == request.Body.ChatId)
            .Where(entity => entity.Id == new ObjectId(request.Body.Id))
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (note == null)
        {
            return response.BadRequest("Note not found");
        }

        note.Status = (int)EventStatus.Deleted;

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return response.Success("Note deleted successfully.");
    }
}