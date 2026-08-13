using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.Utils;

namespace ZiziBot.Application.Features.Handlers.RestApis.Note;

public class DeleteNoteRequest : ApiRequestBase<bool>
{
    [FromBody]
    public DeleteNoteRequestBody Body { get; set; }
}

public class DeleteNoteRequestBody
{
    public long ChatId { get; set; }
    public string Id { get; set; }
}

public class DeleteNoteHandler(
    DataFacade dataFacade
) : IApiRequestHandler<DeleteNoteRequest, bool>
{
    public async Task<ApiResponseBase<bool>> Handle(DeleteNoteRequest request, CancellationToken cancellationToken)
    {
        ApiResponseBase<bool> response = new();

        var note = await dataFacade.MongoDb.Note
            .Where(entity => entity.ChatId == request.Body.ChatId)
            .Where(entity => entity.Id == request.Body.Id.ToObjectId())
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (note == null)
        {
            return response.BadRequest("Note not found");
        }

        note.Status = EventStatus.Deleted;

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return response.Success("Note deleted successfully.");
    }
}
