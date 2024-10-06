using Microsoft.Extensions.Logging;
using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Mirror;

public class SavePaymentBotRequestModel : BotRequestBase
{
    public string? Payload { get; set; }
    public long ForUserId { get; set; }
}

public class SavePaymentRequestHandler(
    ILogger<SavePaymentRequestHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestHandler<SavePaymentBotRequestModel, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(SavePaymentBotRequestModel request, CancellationToken cancellationToken)
    {
        var htmlMessage = HtmlMessage.Empty;
        serviceFacade.TelegramService.SetupResponse(request);

        var userId = request.UserId;

        if (request.ReplyToMessage == null)
        {
            return await serviceFacade.TelegramService.SendMessageText("Balas sebuah pesan untuk menambahkan pengguna.");
        }

        var replyToMessage = request.ReplyToMessage;
        var messageId = replyToMessage.MessageId.ToString();
        var fileId = replyToMessage.GetFileId();
        var transactionId = $"{messageId}-{fileId}";

        if (request.Payload.TryConvert(1, out var cendolCount))
        {
            if (replyToMessage.ForwardSenderName != null &&
                request.ForUserId == 0)
            {
                htmlMessage.Text("Sepertinya Pengguna disembunyikan, spesifikan ID pengguna.").Br()
                    .Bold("Contoh: ").Code($"/mp {cendolCount} (userId)");

                return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
            }

            var mirrorApproval = await dataFacade.MongoDb.MirrorApproval
                .Where(entity => entity.TransactionId == transactionId)
                .Where(entity => entity.Status == (int)EventStatus.Complete)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (mirrorApproval != null)
            {
                return await serviceFacade.TelegramService.SendMessageText("Sepertinya tiket ini sudah diklaim");
            }

            await serviceFacade.TelegramService.SendMessageText("Sedang menambahkan pengguna...");
            dataFacade.MongoDb.MirrorApproval.Add(new MirrorApprovalEntity() {
                UserId = request.UserId,
                RawText = request.ReplyToMessage.Text,
                OrderId = messageId,
                FileId = request.ReplyToMessage.GetFileId(),
                Status = (int)EventStatus.Complete,
                TransactionId = transactionId
            });
        }
        else
        {
            htmlMessage.Text("Sertakan durasi berlangganan dengan benar.").Br()
                .Bold("Contoh: ").Code("/mp (duration) [userId]");

            return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
        }

        if (request.ForUserId != 0)
        {
            userId = request.ForUserId;
        }

        var forwardMessage = replyToMessage.ForwardFrom;
        if (forwardMessage != null)
        {
            userId = forwardMessage.Id;
        }

        var mirrorUser = await dataFacade.MongoDb.MirrorUsers
            .Where(entity => entity.UserId == userId)
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var expireDate = DateTime.Now.AddMonths(cendolCount);

        if (mirrorUser == null)
        {
            logger.LogInformation("Creating Mirror subscription for user {UserId} with Expire date: {Date}", userId,
                expireDate);

            dataFacade.MongoDb.MirrorUsers.Add(new MirrorUserEntity() {
                UserId = userId,
                ExpireDate = expireDate,
                Status = (int)EventStatus.Complete,
                TransactionId = transactionId
            });
        }
        else
        {
            logger.LogInformation("Extending Mirror subscription for user {UserId} with Expire date: {Date}", userId,
                expireDate);

            expireDate = mirrorUser.ExpireDate < DateTime.Now ?
                expireDate // If expired, will be started from now
                :
                mirrorUser.ExpireDate
                    .AddMonths(cendolCount); // If not expired, it will be extended from current expire date

            mirrorUser.ExpireDate = expireDate;
            mirrorUser.Status = (int)EventStatus.Complete;
            mirrorUser.TransactionId = transactionId;
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        htmlMessage.Bold("Langganan Mirror").Br()
            .Bold("ID Pengguna: ").Code(userId.ToString()).Br()
            .Bold("Jumlah Cendol: ").Code(cendolCount.ToString()).Br()
            .Bold("Masa Aktif: ").Code(expireDate.AddHours(Env.DEFAULT_TIMEZONE).ToString("yyyy-MM-dd HH:mm:ss")).Br();

        await serviceFacade.TelegramService.EditMessageText(htmlMessage.ToString());

        var mirrorConfig = await dataFacade.AppSetting.GetConfigSectionAsync<MirrorConfig>();

        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString(), chatId: mirrorConfig.ApprovalChannelId,
            threadId: 0);
    }
}