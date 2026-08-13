using Microsoft.Extensions.Logging;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.Presentation.Bots.Telegram.Controllers;

// [BotName("Main")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class InlineQueryController(
    ILogger<InlineQueryController> logger,
    MediatorService mediatorService
) : BotCommandController
{
    [InlineQuery()]
    public async Task InlineSearchBotApi(CommandContext context)
    {
        var inlineCmd = context.InlineQuery.Query.GetInlineQueryAt<string>(0);

        var result = inlineCmd switch
        {
            "uup" => await mediatorService.EnqueueAsync(new AnswerInlineQueryUupBotRequestModel()
            {
                BotToken = context.BotToken,
                InlineQuery = context.InlineQuery,
                Query = context.InlineQuery.Query.GetInlineQueryAt<string>(1)
            }),
            "search" => await mediatorService.EnqueueAsync(new AnswerInlineQueryWebSearchBotRequestModel()
            {
                BotToken = context.BotToken,
                InlineQuery = context.InlineQuery,
                Query = context.InlineQuery.Query.Replace(inlineCmd, "").Trim()
            }),
            "subdl" => await mediatorService.EnqueueAsync(new AnswerInlineQuerySubdlRequest()
            {
                BotToken = context.BotToken,
                InlineQuery = context.InlineQuery
            }),
            _ => await mediatorService.EnqueueAsync(new AnswerInlineQueryGuideBotRequestModel()
            {
                BotToken = context.BotToken,
                InlineQuery = context.InlineQuery
            })
        };

        logger.LogInformation("InlineSearchBotApi: {@Result}", result);
    }

    [InlineQuery("api-doc")]
    public async Task SearchApiDoc(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new AnswerInlineQueryApiDocBotRequestModel()
        {
            BotToken = context.BotToken,
            InlineQuery = context.InlineQuery,
            Query = context.InlineQueryQueryParam
        });
    }

    [InlineQuery("anteraja")]
    [InlineQuery("jne")]
    [InlineQuery("jnt")]
    [InlineQuery("lion")]
    [InlineQuery("ncs")]
    [InlineQuery("tiki")]
    [InlineQuery("trawl")]
    [InlineQuery("trawlbens")]
    [InlineQuery("sicepat")]
    [InlineQuery("wahana")]
    public async Task SearchResi(CommandContext context)
    {
        await mediatorService.EnqueueAsync(new CheckAwbInlineRequest()
        {
            BotToken = context.BotToken,
            Message = context.Message,
            InlineQuery = context.InlineQuery,
            ReplyMessage = true,
        });
    }

    public async Task WebSearch(CommandContext context)
    {
    }
}

