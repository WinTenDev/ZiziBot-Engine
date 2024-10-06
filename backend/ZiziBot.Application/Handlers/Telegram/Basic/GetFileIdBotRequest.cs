using Telegram.Bot.Types.Enums;

namespace ZiziBot.Application.Handlers.Telegram.Basic;

public class GetFileIdBotRequest : BotRequestBase
{ }

public class GetFileIdHandler(
    ServiceFacade serviceFacade
) : IRequestHandler<GetFileIdBotRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(GetFileIdBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var htmlMessage = HtmlMessage.Empty;

        if (request.ReplyToMessage == null)
        {
            return await serviceFacade.TelegramService.SendMessageText("Balas sebuah pesan untuk mendapatkan File ID-nya");
        }

        if (request.ChatType != ChatType.Private)
        {
            htmlMessage.BoldBr($"👥 {request.ChatTitle}")
                .Bold("Chat ID: ").CodeBr(request.ChatId.ToString())
                .Br();
        }

        var replyToMessage = request.ReplyToMessage;
        var fileId = replyToMessage.GetFileId();

        if (fileId.IsNullOrEmpty())
        {
            return await serviceFacade.TelegramService.SendMessageText("Tidak dapat menemukan File ID");
        }

        htmlMessage.Bold("FileId: ").CodeBr(fileId)
            .Bold("Type: ").CodeBr(replyToMessage.Type.ToString());


        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
    }
}