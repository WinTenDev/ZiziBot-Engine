using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.Application.Infrastructure.Database.MongoDb;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;
using ZiziBot.Application.Features.Hangfire;

namespace ZiziBot.Application.Features.UseCases.Rss;

public sealed class FetchRssUseCase(
    ILogger<FetchRssUseCase> logger,
    MongoDbContext mongoDbContext,
    BotRepository botRepository,
    FeatureFlagRepository featureFlagRepository,
    RssRepository rssRepository,
    ReadRssUseCase readRssUseCase
)
{
    public async Task<bool> Handle(long chatId, int? threadId, string rssUrl)
    {
        if (!await featureFlagRepository.IsEnabled(Flag.RSS_BROADCASTER))
            return true;

        logger.LogInformation("Processing RSS Url: {Url}", rssUrl);
        var sentMessage = new Message();
        var trxId = Guid.CreateVersion7().ToString();
        var botSettings = await botRepository.GetBotMain();

        try
        {
            var botClient = new TelegramBotClient(botSettings.Token);
            var feed = await readRssUseCase.Handle(rssUrl);

            if (feed.Items.IsEmpty())
            {
                logger.LogWarning("No RSS article found for ChatId: {ChatId}. Url: {Url}", chatId, rssUrl);
                return true;
            }

            foreach (var latestArticle in feed.Items.Take(3))
            {
                var latestHistory = await rssRepository.GetLastRssArticle(chatId, threadId, latestArticle.Link);

                if (latestHistory != null)
                {
                    logger.LogDebug("Article for ChatId: {ChatId} is already sent: {Url}", chatId, latestHistory.Url);
                    continue;
                }

                try
                {
                    sentMessage = await botClient.SendMessage(
                        chatId: chatId,
                        messageThreadId: threadId,
                        text: latestArticle.Content,
                        parseMode: ParseMode.Html,
                        linkPreviewOptions: true
                    );
                }
                catch (Exception exception)
                {
                    if (exception.Message.Contains("thread not found"))
                    {
                        logger.LogWarning(exception, "Trying send RSS without thread to ChatId: {ChatId}", chatId);
                        sentMessage = await botClient.SendMessage(
                            chatId: chatId,
                            text: latestArticle.Content,
                            parseMode: ParseMode.Html,
                            linkPreviewOptions: true
                        );
                    }
                }

                mongoDbContext.RssHistory.Add(new RssHistoryEntity {
                    ChatId = chatId,
                    ThreadId = threadId,
                    RssUrl = rssUrl,
                    Title = latestArticle.Title,
                    Url = latestArticle.Link,
                    Author = latestArticle.Author,
                    PublishDate = latestArticle.PublishDate,
                    Status = EventStatus.Complete,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    HistoryType = HistoryType.Success,
                    HistoryTypeName = nameof(HistoryType.Success),
                    TransactionId = trxId
                });
            }

            var rssSetting = await mongoDbContext.RssSetting
                .Where(x => x.ChatId == chatId)
                .WhereIf(threadId > 0, x => x.ThreadId == threadId)
                .Where(x => x.RssUrl == rssUrl)
                .FirstOrDefaultAsync();

            if (rssSetting != null && sentMessage.MessageId > 0)
            {
                rssSetting.LastSuccessDate = DateTime.UtcNow;
            }
        }
        catch (Exception exception)
        {
            var exceptionMessage = exception.InnerException?.Message ?? exception.Message;

            if (exception.IsIgnorable())
            {
                logger.LogWarning("Error while sending RSS for ChatId: {ChatId}, Url: {Url}. Reason: {Message}", chatId, rssUrl, exception.Message);
                var findRssSetting = await mongoDbContext.RssSetting
                    .Where(entity => entity.ChatId == chatId)
                    .Where(entity => entity.RssUrl == rssUrl)
                    .Where(entity => entity.Status == EventStatus.Complete)
                    .ToListAsync();


                findRssSetting.ForEach(rss =>
                {
                    rss.Status = EventStatus.InProgress;
                    rss.LastErrorMessage = exceptionMessage;
                    rss.TransactionId = trxId;

                    HangfireUtil.RemoveRecurringJob(rss.CronJobId);
                    logger.LogWarning("Removed RSS CronJob for ChatId: {ChatId}, Url: {Url}. Reason: {Message}", rss.ChatId, rss.RssUrl, exceptionMessage);
                });
            }
            else
            {
                logger.LogError(exception, "Error while sending RSS article to Chat: {ChatId}. Url: {Url}", chatId, rssUrl);
            }

            mongoDbContext.RssHistory.Add(new RssHistoryEntity {
                ChatId = chatId,
                ThreadId = threadId,
                RssUrl = rssUrl,
                Status = EventStatus.Complete,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Exception = exception.GetType().Name,
                ErrorMessage = exceptionMessage,
                HistoryType = HistoryType.Error,
                HistoryTypeName = nameof(HistoryType.Error),
                TransactionId = trxId
            });
        }

        await mongoDbContext.SaveChangesAsync();

        return true;
    }
}
