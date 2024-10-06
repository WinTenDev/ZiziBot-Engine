using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.RestApis.Note;

public class SaveNoteRequest : ApiRequestBase<bool>
{
    [FromBody] public SaveNoteRequestModel Model { get; set; }
}

public class SaveNoteRequestModel
{
    public string? Id { get; set; }
    public long ChatId { get; set; }
    public string Query { get; set; }
    public string Content { get; set; }
    public string? FileId { get; set; }
    public string? RawButton { get; set; }
    public int DataType { get; set; } = -1;

    [BindNever]
    [SwaggerIgnore]
    public ObjectId ObjectId => Id != null ?
        new ObjectId(Id) :
        ObjectId.Empty;
}

public class SaveNoteValidator : AbstractValidator<SaveNoteRequest>
{
    public SaveNoteValidator()
    {
        RuleFor(x => x.Model.ChatId).NotEqual(0).WithMessage("ChatId is required");
        RuleFor(x => x.Model.Query).NotNull().WithMessage("Slug is required");
    }
}

public class CreateNoteHandler(
    IHttpContextAccessor httpContextAccessor,
    ServiceFacade serviceFacade
) : IRequestHandler<SaveNoteRequest, ApiResponseBase<bool>>
{
    public async Task<ApiResponseBase<bool>> Handle(SaveNoteRequest request, CancellationToken cancellationToken)
    {
        ApiResponseBase<bool> response = new();

        if (!request.ListChatId.Contains(request.Model.ChatId))
        {
            return response.BadRequest("You don't have permission to create note for this Chat");
        }

        var save = await serviceFacade.NoteService.Save(new NoteEntity() {
            Id = request.Model.ObjectId,
            ChatId = request.Model.ChatId,
            Query = request.Model.Query,
            Content = request.Model.Content,
            FileId = request.Model.FileId,
            RawButton = request.Model.RawButton,
            DataType = request.Model.DataType,
            Status = (int)EventStatus.Complete,
            UserId = httpContextAccessor.GetUserId(),
            TransactionId = httpContextAccessor.GetTransactionId()
        });

        return response.Success(save.Message, true);
    }
}