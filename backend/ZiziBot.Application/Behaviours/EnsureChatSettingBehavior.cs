using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Behaviours;

public class EnsureChatSettingBehavior<TRequest, TResponse>(
    ILogger<EnsureChatSettingBehavior<TRequest, TResponse>> logger,
    MongoDbContextBase mongoDbContext)
    : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : BotRequestBase, IRequest<TResponse>
    where TResponse : BotResponseBase
{
    public async Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (request.ChatId == 0)
            return;

        logger.LogInformation("Ensure ChatSetting for ChatId: {ChatId} Started", request.ChatId);

        var chatSetting = await mongoDbContext.ChatSetting
            .FirstOrDefaultAsync(x => x.ChatId == request.ChatIdentifier, cancellationToken: cancellationToken);

        if (chatSetting == null)
        {
            logger.LogDebug("Creating fresh ChatSetting for ChatId: {ChatId}", request.ChatId);

            mongoDbContext.ChatSetting.Add(
                new ChatSettingEntity() {
                    ChatId = request.ChatIdentifier,
                    ChatTitle = request.ChatTitle,
                    ChatType = request.ChatType,
                    ChatTypeName = request.ChatType.ToString(),
                    Status = (int)EventStatus.Complete
                }
            );
        }
        else
        {
            logger.LogDebug("Updating ChatSetting for ChatId: {ChatId}", request.ChatId);

            chatSetting.ChatTitle = request.ChatTitle;
            chatSetting.ChatType = request.ChatType;
            chatSetting.ChatTypeName = request.ChatType.ToString();
            chatSetting.Status = (int)EventStatus.Complete;
        }

        await mongoDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Ensure ChatSetting for ChatId: {ChatId} Done", request.ChatId);
    }
}