using Flurl;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using ZiziBot.Types.Vendor.Subdl;

namespace ZiziBot.Application.Handlers.Telegram.Inline;

public class AnswerInlineQuerySubdlRequest : BotRequestBase
{ }

public class AnswerInlineQuerySubdlHandler(
    ILogger<AnswerInlineQuerySubdlHandler> logger,
    ServiceFacade serviceFacade
) : IBotRequestHandler<AnswerInlineQuerySubdlRequest>
{
    public async Task<BotResponseBase> Handle(AnswerInlineQuerySubdlRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);
        Popular popular = new();
        IEnumerable<InlineQueryResult>? inlineQueryResults = default;

        logger.LogInformation("Find subdl for Query: {query}", request.InlineParam);
        if (request.InlineParam.IsNullOrEmpty())
        {
            popular = await serviceFacade.SubdlService.FetchPopular();
        }
        else
        {
            popular = await serviceFacade.SubdlService.Search(request.InlineParam);
        }

        inlineQueryResults = popular.Results?.Select(x => {
            var slugOrPath = x.Slug.IsNullOrEmpty() ?
                x.Link :
                "subtitle".AppendPathSegments(x.SdId, x.Slug).ToString();

            var subtitleUrl = $"https://subdl.com/".AppendPathSegments(slugOrPath);
            var htmlContent = HtmlMessage.Empty
                .CodeBr(x.Name)
                .Bold("Year : ").CodeBr(x.Year.ToString())
                .TextBr(subtitleUrl);

            var replyMarkup = new InlineKeyboardMarkup(new[] {
                new[] {
                    InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Pencarian baru", $"subdl "),
                    InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Pencarian lanjut", $"subdl {request.InlineParam}")
                }
            });

            var name = $"{x.Year} - {x.Name}";
            var fields = new InlineQueryResultArticle(
                id: "subdl-" + StringUtil.GetNanoId(),
                title: name,
                inputMessageContent: new InputTextMessageContent(htmlContent.ToString()) {
                    ParseMode = ParseMode.Html,
                    DisableWebPagePreview = false
                }
            ) {
                Description = subtitleUrl,
                ThumbnailUrl = x.PosterUrl,
                ReplyMarkup = replyMarkup
            };

            return fields;
        });


        return await serviceFacade.TelegramService.AnswerInlineQueryAsync(inlineQueryResults);
    }
}