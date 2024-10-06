using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Group;

public class ThreadUpdateBotRequest : BotRequestBase
{ }

public class ThreadUpdateHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IBotRequestHandler<ThreadUpdateBotRequest>
{
    public async Task<BotResponseBase> Handle(ThreadUpdateBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var prevTopicName = string.Empty;
        var htmlMessage = HtmlMessage.Empty;

        var findTopic = await dataFacade.MongoDb.GroupTopic
            .Where(entity => entity.ChatId == request.ChatIdentifier)
            .Where(entity => entity.ThreadId == request.MessageThreadId)
            .Where(entity => entity.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (!request.CreatedTopicName.IsNullOrEmpty())
            htmlMessage.Text("Topic telah dibuat").Br()
                .Bold("Name: ").Code(request.TopicName).Br();

        if (!request.EditedTopicName.IsNullOrEmpty())
            htmlMessage.Text("Topic berubah nama").Br()
                .Bold("Sesudah: ").Code(request.TopicName).Br();

        if (findTopic == null)
        {
            var entity = new GroupTopicEntity() {
                ChatId = request.ChatIdentifier,
                ThreadId = request.MessageThreadId,
                Status = (int)EventStatus.Complete
            };

            if (request.TopicName != null)
                entity.ThreadName = request.TopicName;

            dataFacade.MongoDb.GroupTopic.Add(entity);
        }
        else
        {
            if (request.TopicName != null)
            {
                prevTopicName = findTopic.ThreadName;
                findTopic.ThreadName = request.TopicName;

                htmlMessage.Bold("Sebelum: ").Code(prevTopicName).Br();
            }
        }

        htmlMessage.Bold("Topic ID: ").Code(request.MessageThreadId.ToString());

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
    }
}