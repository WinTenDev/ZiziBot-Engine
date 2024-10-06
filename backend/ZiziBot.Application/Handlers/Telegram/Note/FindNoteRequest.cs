using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Handlers.Telegram.Note;

public class FindNoteRequestHandler<TRequest, TResponse>(
    ILogger<FindNoteRequestHandler<TRequest, TResponse>> logger,
    ServiceFacade serviceFacade
)
    : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : BotRequestBase, IRequest<TResponse>
    where TResponse : BotResponseBase
{
    public async Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (request.Source != ResponseSource.Bot ||
            request.ChatId == 0)
        {
            logger.LogDebug("Find Note stop because Source: {Source}", request.Source);

            return;
        }

        request.ReplyMessage = true;
        request.CleanupTargets = new[] {
            CleanupTarget.None
        };

        serviceFacade.TelegramService.SetupResponse(request);

        var listNote = await serviceFacade.NoteService.GetAllByChat(request.ChatIdentifier);

        if (listNote.Count == 0)
        {
            logger.LogInformation("No note found in chat {chatId}", request.ChatIdentifier);
            return;
        }

        var words = request.MessageTexts?.Where(x => x.StartsWith('#')).ToList();

        if (words?.Any() ?? false)
        {
            //find note single word - invoked by hashtag (#)
            foreach (var notes in words
                         .Select(word => word.Replace("#", ""))
                         .Select(tag => listNote.FirstOrDefault(x => x.Query == tag))
                         .Where(notes => notes != null)
                    )
                await SendNoteAsync(notes);
        }
        else
        {
            //find note by text - invoked by entire/partial message text
            var note = listNote.FirstOrDefault(x =>
                request.MessageText?.Equals(x.Query, StringComparison.CurrentCultureIgnoreCase) ?? false);

            await SendNoteAsync(note);
        }


        async Task SendNoteAsync(NoteDto? notes)
        {
            if (notes == null)
            {
                logger.LogDebug("No Notes for Send to ChatId: {ChatId}", request.ChatId);
                return;
            }

            var dataType = (CommonMediaType)notes.DataType;

            logger.LogInformation("Sending note {NoteId} with data type {DataType}", notes.Id, dataType);

            var replyMarkup = notes.RawButton.ToButtonMarkup();

            if (dataType <= CommonMediaType.Text)
                await serviceFacade.TelegramService.SendMessageText(notes.Text, replyMarkup);
            else
                await serviceFacade.TelegramService.SendMediaAsync(
                    notes.FileId,
                    caption: notes.Text,
                    mediaType: (CommonMediaType)notes.DataType,
                    replyMarkup: replyMarkup
                );
        }
    }
}