using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Handlers.Telegram.Middleware;

public class ActionResultPipelineBehavior<TRequest, TResponse>(
    ILogger<ActionResultPipelineBehavior<TRequest, TResponse>> logger,
    ServiceFacade serviceFacade,
    DataFacade dataFacade
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : BotRequestBase
    where TResponse : BotResponseBase, new()
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.Source != ResponseSource.Bot ||
            request.PipelineResult.Actions.IsEmpty())
            return await next();

        var actions = request.PipelineResult.Actions;
        var htmlMessage = HtmlMessage.Empty
            .User(request.UserId, request.User.GetFullName());

        serviceFacade.TelegramService.SetupResponse(request);

        if (actions.Contains(PipelineResultAction.Warn))
        {
            htmlMessage.Text(" telah diperingatkan karena: ").Br();
            if (!request.PipelineResult.IsMessagePassed)
                htmlMessage.Text(" - mengirim pesan yang mengandung teks yang dilarang");

            if (!request.PipelineResult.HasUsername)
                htmlMessage.Text(" - belum menetapkan Username").Br();
        }

        try
        {
            //todo. incremental
            var muteDuration = MemberMuteDuration.Select(0);

            if (actions.Count != 0)
                htmlMessage.BoldBr("Aksi: ");

            if (actions.Contains(PipelineResultAction.Mute))
            {
                await serviceFacade.TelegramService.MuteMemberAsync(request.UserId, muteDuration);
                htmlMessage.TextBr($" - pengguna disenyapkan selama {muteDuration.ForHuman()}");
            }

            if (actions.Contains(PipelineResultAction.Delete))
            {
                await serviceFacade.TelegramService.DeleteMessageAsync();
                htmlMessage.TextBr(" - pesan dihapus");
            }

            if (actions.Contains(PipelineResultAction.Kick))
            {
                await serviceFacade.TelegramService.KickMember();
                htmlMessage.TextBr(" - pengguna dikeluarkan dari grub");
            }

            await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error when running action for userId: {UserId}. Message: {Message}", request.UserId, exception.Message);
        }

        return new TResponse();
    }
}