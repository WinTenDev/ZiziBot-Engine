using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Features.Handlers.Telegram.Inline;

public class AnswerInlineQueryWebSearchBotRequestModel : BotRequestBase
{
    public string? Query { get; set; }
}

public class AnswerInlineQueryWebSearchRequestHandler(
    ServiceFacade serviceFacade
)
    : IBotRequestHandler<AnswerInlineQueryWebSearchBotRequestModel>
{
    public async Task<BotResponseBase> Handle(AnswerInlineQueryWebSearchBotRequestModel request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        if (request.Query.IsNullOrEmpty())
        {
            return await serviceFacade.TelegramService.AnswerInlineQueryAsync(new List<InlineQueryResult>() {
                new InlineQueryResultArticle(
                    id: "guide-1",
                    title: "Ketikkan sebuah kueri untuk memulai pencarian..",
                    inputMessageContent: new InputTextMessageContent(InlineDefaults.DefaultGuideText) {
                        LinkPreviewOptions = true
                    }
                ) {
                    ReplyMarkup = InlineDefaults.DefaultButtonMarkup
                },
            });
        }

        var search = await WebParserUtil.WebSearchText(request.Query);

        var inlineResult = search.Select(x => {
            var htmlContent = HtmlMessage.Empty
                .CodeBr(x.Title)
                .TextBr(x.Url);

            var replyMarkup = new InlineKeyboardMarkup(new[] {
                new[] {
                    InlineKeyboardButton.WithUrl("↗️ Open", x.Url)
                },
                new[] {
                    InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Pencarian baru", $"search "),
                    InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Pencarian lanjut", $"search {request.Query}")
                }
            });

            var fields = new InlineQueryResultArticle(
                id: $"search-{Guid.NewGuid().ToString()}",
                title: x.Title,
                inputMessageContent: new InputTextMessageContent(htmlContent.ToString()) {
                    ParseMode = ParseMode.Html,
                    LinkPreviewOptions = false
                }
            ) {
                Description = x.Url,
                ReplyMarkup = replyMarkup
            };

            return fields;
        });

        return await serviceFacade.TelegramService.AnswerInlineQueryAsync(inlineResult);
    }
}