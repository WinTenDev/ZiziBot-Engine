using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace ZiziBot.Application.Features.Handlers.Telegram.Additional;

public class CheckAwbInlineRequest : BotRequestBase
{ }

public class CheckAwbInlineHandler(
    ServiceFacade serviceFacade
)
    : IBotRequestHandler<CheckAwbInlineRequest>
{
    public async Task<BotResponseBase> Handle(CheckAwbInlineRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);
        var courier = request.InlineCommand.GetCommandParamAt<string>(0);
        var awb = request.InlineParam.GetCommandParamAt<string>(0);

        if (request.MessageTexts?.Length > 2)
        {
            return serviceFacade.TelegramService.Complete();
        }

        if (courier == null || awb == null)
        {
            return await serviceFacade.TelegramService.AnswerInlineQueryAsync(new List<InlineQueryResult>() {
                new InlineQueryResultArticle() {
                    Id = "awb-default",
                    Title = "Berapa no Resi nya?",
                    InputMessageContent = new InputTextMessageContent() {
                        MessageText = "Masukkan No Resi"
                    }
                }
            });
        }

        var check = await serviceFacade.TonjooService.GetAwbInfoMerged(courier, awb);
        if (check.Contains("tidak ada"))
        {
            check = await serviceFacade.BinderByteService.CekResiMergedAsync(courier, awb);
        }

        return await serviceFacade.TelegramService.AnswerInlineQueryAsync(new List<InlineQueryResult>() {
            new InlineQueryResultArticle() {
                Id = "awb-default",
                Title = "No Resi ditemukan",
                InputMessageContent = new InputTextMessageContent() {
                    MessageText = check,
                    ParseMode = ParseMode.Html
                }
            }
        });
    }
}