using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Group.Welcome;

public class SaveWelcomeMessageRequest : BotRequestBase
{ }

public class SaveWelcomeMessageHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestHandler<SaveWelcomeMessageRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(SaveWelcomeMessageRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        if (request.ReplyToMessage == null)
        {
            return await serviceFacade.TelegramService.SendMessageAsync("Balas pesan untuk disimpan");
        }

        await serviceFacade.TelegramService.SendMessageAsync("Sedang menyimpan..");

        var welcomeMessage = await dataFacade.MongoDb.WelcomeMessage
            .Where(e => e.ChatId == request.ChatIdentifier)
            .Where(e => e.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync();

        if (welcomeMessage == null)
        {
            dataFacade.MongoDb.WelcomeMessage.Add(new WelcomeMessageEntity() {
                ChatId = request.ChatIdentifier,
                UserId = request.User!.Id,
                Status = (int)EventStatus.Complete,
            });

            await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

            welcomeMessage = await dataFacade.Group.GetWelcomeMessage(request.ChatIdentifier);
        }

        switch (request.Command)
        {
            case "/wt":
                welcomeMessage.Text = request.ReplyToMessage.Text;
                break;
            case "/wd":
                welcomeMessage.Media = request.ReplyToMessage.GetFileId();
                welcomeMessage.DataType = (int)request.ReplyToMessage.Type;
                break;
            case "/wb":
                welcomeMessage.RawButton = request.ReplyToMessage.Text;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return await serviceFacade.TelegramService.SendMessageAsync($"Berhasil menyimpan. {request.Command}");
    }
}