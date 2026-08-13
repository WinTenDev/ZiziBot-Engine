using Microsoft.Extensions.Logging;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Infrastructure.Pipelines.PrePipeline;

public class ActionResultPipeline<TRequest, TResponse>(
    ILogger<ActionResultPipeline<TRequest, TResponse>> logger,
    ServiceFacade serviceFacade
) : ITelegramPreProcessPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<PreProcessResult<TResponse>> ProcessAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (request is not BotRequestBase botRequest ||
            ShouldSkip(botRequest))
            return PreProcessResult<TResponse>.Continue;

        var actions = botRequest.PipelineResult.Actions;
        var htmlMessage = BuildWarningMessage(botRequest, actions);

        serviceFacade.TelegramService.SetupResponse(botRequest);

        try
        {
            await ApplyActionsAsync(botRequest, actions, htmlMessage);

            return PreProcessResult<TResponse>.Stop();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error on Action Result pipeline");
            return PreProcessResult<TResponse>.Stop();
        }
    }

    private static bool ShouldSkip(BotRequestBase botRequest)
    {
        if (botRequest.Source != ResponseSource.Bot ||
            botRequest.PipelineResult.Actions.IsEmpty())
            return true;

        //todo. auto detect based on user activity
        return ValueConst.SAFE_IDS.Contains(botRequest.UserId);
    }

    private static HtmlMessage BuildWarningMessage(BotRequestBase botRequest, ICollection<PipelineResultAction> actions)
    {
        var htmlMessage = HtmlMessage.Empty
            .Bold("ID: ").CodeBr(botRequest.UserId.ToString())
            .Bold("Name: ").User(botRequest.UserId, botRequest.User.GetFullName());

        if (!actions.Contains(PipelineResultAction.Warn))
            return htmlMessage;

        htmlMessage.Br().Text("Mendapat diperingatkan karena: ").Br();

        if (!botRequest.PipelineResult.IsMessagePassed)
            htmlMessage.Text(" - mengirim pesan yang mengandung teks yang dilarang").Br();

        if (!botRequest.PipelineResult.HasUsername)
            htmlMessage.Text(" - belum menetapkan Username").Br();

        return htmlMessage;
    }

    private async Task ApplyActionsAsync(
        BotRequestBase botRequest,
        ICollection<PipelineResultAction> actions,
        HtmlMessage htmlMessage
    )
    {
        var muteDuration = MemberMuteDuration.Select(0);
        var actionResults = HtmlMessage.Empty;

        if (actions.Count != 0)
            htmlMessage.Br().Br();

        if (actions.Contains(PipelineResultAction.Delete))
            await TryDeleteMessageAsync(botRequest, actionResults);

        if (actions.Contains(PipelineResultAction.Mute))
            await TryMuteMemberAsync(botRequest, muteDuration, actionResults);

        if (actions.Contains(PipelineResultAction.Kick))
            await TryKickMemberAsync(botRequest, htmlMessage);

        if (!actionResults.IsEmpty)
            htmlMessage.BoldBr("Aksi: ").Append(actionResults);

        if (actions.Contains(PipelineResultAction.Warn))
            await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
    }

    private async Task TryDeleteMessageAsync(BotRequestBase botRequest, HtmlMessage actionResults)
    {
        try
        {
            await serviceFacade.TelegramService.DeleteMessageAsync();
            actionResults.TextBr(" - pesan dihapus");
        }
        catch (Exception e)
        {
            logger.LogWarning("Unable to delete message: {MessageId} in ChatId: {ChatId}. Message: {Message}",
                botRequest.MessageId, botRequest.ChatId, e.Message);
        }
    }

    private async Task TryMuteMemberAsync(BotRequestBase botRequest, TimeSpan muteDuration, HtmlMessage actionResults)
    {
        try
        {
            await serviceFacade.TelegramService.MuteMemberAsync(botRequest.UserId, muteDuration);
            actionResults.TextBr($" - pengguna disenyapkan selama {muteDuration.ForHuman()}");
        }
        catch (Exception e)
        {
            logger.LogWarning("Unable to mute user: {UserId} in ChatId: {ChatId}. Message: {Message}", botRequest.UserId, botRequest.ChatId, e.Message);
        }
    }

    private async Task TryKickMemberAsync(BotRequestBase botRequest, HtmlMessage htmlMessage)
    {
        try
        {
            await serviceFacade.TelegramService.KickMember();
            htmlMessage.TextBr(" - pengguna dikeluarkan dari grub");
        }
        catch (Exception e)
        {
            logger.LogWarning("Unable to kick user: {UserId} in ChatId: {ChatId}. Message: {Message}", botRequest.UserId, botRequest.ChatId, e.Message);
        }
    }
}
