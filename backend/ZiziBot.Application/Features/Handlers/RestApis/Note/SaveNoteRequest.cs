using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;
using ZiziBot.Application.Infrastructure.Database.Utils;

namespace ZiziBot.Application.Features.Handlers.RestApis.Note;

public class SaveNoteRequest : ApiRequestBase<bool>
{
    [FromBody]
    public SaveNoteRequestModel Model { get; set; }
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
}

public class SaveNoteValidator : AbstractValidator<SaveNoteRequest>
{
    public SaveNoteValidator()
    {
        RuleFor(x => x.Model.Id).Must(x => ObjectId.TryParse(x,out _)).When(x => !string.IsNullOrWhiteSpace(x.Model.Id)).WithMessage("Id is invalid");
        RuleFor(x => x.Model.ChatId).NotEqual(0).WithMessage("ChatId is required");
        RuleFor(x => x.Model.Query).NotNull().WithMessage("Slug is required");
    }
}

public class CreateNoteHandler(
    IHttpContextHelper httpContextHelper,
    ServiceFacade serviceFacade,
    DataFacade dataFacade
) : IApiRequestHandler<SaveNoteRequest, bool>
{
    public async Task<ApiResponseBase<bool>> Handle(SaveNoteRequest request, CancellationToken cancellationToken)
    {
        ApiResponseBase<bool> response = new();

        if (!httpContextHelper.UserInfo.ListChatId.Contains(request.Model.ChatId))
        {
            return response.BadRequest("You don't have permission to create note for this Chat");
        }

        var save = await dataFacade.ChatSetting.Save(new NoteEntity {
            Id = request.Model.Id.ToObjectId(),
            ChatId = request.Model.ChatId,
            Query = request.Model.Query,
            Content = request.Model.Content,
            FileId = request.Model.FileId,
            RawButton = request.Model.RawButton,
            DataType = request.Model.DataType,
            Status = EventStatus.Complete,
            UserId = httpContextHelper.UserInfo.UserId,
            TransactionId = httpContextHelper.TransactionId
        });

        return response.Success(save.Message, true);
    }
}
