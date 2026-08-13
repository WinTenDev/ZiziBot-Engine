using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Common.Types;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.Telegram.Note;

public class CreateNoteBotRequest : BotRequestBase
{
    public string Query { get; set; }
    public string? Content { get; set; }
    public string? FileId { get; set; }
    public string? RawButton { get; set; }
    public int DataType { get; set; }
    public bool? RefreshNote { get; set; }
}

public class CreateNoteValidator : AbstractValidator<CreateNoteBotRequest>
{
    public CreateNoteValidator()
    {
        RuleFor(x => x.Query).NotNull().When(x => x.Content != null).WithMessage("Masukkan Query untuk Note nya");
        RuleFor(x => x.Query).MinimumLength(2).WithMessage("Panjang Query minimal 2 karakter");
        RuleFor(x => x.Content).NotNull().When(x => x.FileId == null)
            .WithMessage("Balas sebuah pesan yang mengandung Teks");
    }
}

public class CreateNoteHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IBotRequestHandler<CreateNoteBotRequest>
{
    public async Task<BotResponseBase> Handle(CreateNoteBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var htmlMessage = HtmlMessage.Empty;

        var validationResult = await request.ValidateAsync<CreateNoteValidator, CreateNoteBotRequest>();

        if (!validationResult.IsValid)
        {
            htmlMessage.Text(validationResult.Errors.Select(x => x.ErrorMessage).Aggregate((x, y) => $"{x})\n{y}"));
            return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
        }

        if (request.ReplyToMessage == null)
        {
            await serviceFacade.TelegramService.SendMessageText("Balas sebuah pesan yang akan disimpan");
        }

        var note = await dataFacade.MongoDb.Note
            .Where(entity => entity.ChatId == request.ChatIdentifier)
            .Where(entity => entity.Query == request.Query)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var btnFromMessage = request.Message.GetRawReplyMarkup();

        if (request.RawButton.IsNullOrEmpty())
        {
            request.RawButton = btnFromMessage;
        }

        if (note != null)
        {
            if (request.RefreshNote ?? false)
            {
                await serviceFacade.TelegramService.SendMessageText("Sedang memperbarui catatan...");

                note.Content = request.Content;
                note.FileId = request.FileId;
                note.RawButton = request.RawButton;
                note.DataType = request.DataType;
                note.Status = EventStatus.Complete;
                note.TransactionId = request.TransactionId;
            }
            else
            {
                htmlMessage.Text("Catatan sudah ada, silahkan gunakan nama lainnya. Atau gunakan perintah /renote untuk memperbarui catatan");

                return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
            }
        }
        else
        {
            await serviceFacade.TelegramService.SendMessageText("Sedang membuat catatan...");

            dataFacade.MongoDb.Note.Add(new NoteEntity() {
                ChatId = request.ChatIdentifier,
                UserId = request.UserId,
                Query = request.Query,
                Content = request.Content,
                FileId = request.FileId,
                RawButton = request.RawButton,
                DataType = request.DataType,
                Status = EventStatus.Complete,
                TransactionId = request.TransactionId
            });
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        await serviceFacade.TelegramService.EditMessageText("Memperbarui cache..");
        await dataFacade.ChatSetting.GetAllByChat(request.ChatIdentifier, true);

        return await serviceFacade.TelegramService.EditMessageText("Catatan berhasil disimpan");
    }
}
