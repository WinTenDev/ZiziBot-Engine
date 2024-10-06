using Humanizer;
using Microsoft.Extensions.Logging;
using MongoFramework.Linq;
using MoreLinq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Handlers.Telegram.Rss;

public class FetchRssRequest : IRequest<bool>
{
    public long ChatId { get; set; }
    public int ThreadId { get; set; }
    public string RssUrl { get; set; }
}

public class FetchRssHandler(
    ILogger<FetchRssHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : IRequestHandler<FetchRssRequest, bool>
{
    public async Task<bool> Handle(FetchRssRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing RSS Url: {Url}", request.RssUrl);

        try
        {
            var feed = await request.RssUrl.ReadRssAsync();

            var latestArticle = feed.Items.FirstOrDefault();

            if (latestArticle == null)
            {
                logger.LogInformation("No article found in ChatId: {ChatId} for RSS Url: {Url}", request.ChatId,
                    request.RssUrl);

                return false;
            }

            var latestHistory = await dataFacade.MongoDb.RssHistory.AsNoTracking()
                .Where(entity => entity.ChatId == request.ChatId)
                .Where(entity => entity.ThreadId == request.ThreadId)
                .Where(entity => entity.RssUrl == request.RssUrl)
                .Where(entity => entity.Status == (int)EventStatus.Complete)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (latestHistory != null)
            {
                logger.LogDebug("No new article found in ChatId: {ChatId} for RSS Url: {Url}", request.ChatId,
                    request.RssUrl);

                return false;
            }

            var botSettings = await dataFacade.AppSetting.GetBotMain();

            var botClient = new TelegramBotClient(botSettings.Token);

            var htmlContent = await latestArticle.Content.HtmlForTelegram();

            var messageText = HtmlMessage.Empty
                .Url(feed.Link, feed.Title.Trim()).Br()
                .Url(latestArticle.Link, latestArticle.Title.Trim()).Br();

            if (!request.RssUrl.IsGithubCommitsUrl() &&
                await dataFacade.AppSetting.GetFlagValue(Flag.RSS_INCLUDE_CONTENT))
                messageText.Text(htmlContent.Truncate(2000));

            if (request.RssUrl.IsGithubReleaseUrl())
            {
                var assets = await request.RssUrl.GetGithubAssetLatest();


                if (assets?.Assets.NotEmpty() ?? false)
                {
                    messageText.Br().Br()
                        .BoldBr("Assets");

                    assets.Assets.ForEach(asset => {
                        messageText.Url(asset.BrowserDownloadUrl, asset.Name).Br();
                    });
                }
            }

            var truncatedMessageText = messageText.ToString();

            try
            {
                await botClient.SendTextMessageAsync(
                    chatId: request.ChatId,
                    messageThreadId: request.ThreadId,
                    text: truncatedMessageText,
                    parseMode: ParseMode.Html,
                    disableWebPagePreview: true,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("thread not found"))
                {
                    logger.LogWarning("Trying send RSS without thread to ChatId: {ChatId}", request.ChatId);
                    await botClient.SendTextMessageAsync(
                        chatId: request.ChatId,
                        text: truncatedMessageText,
                        parseMode: ParseMode.Html,
                        disableWebPagePreview: true,
                        cancellationToken: cancellationToken
                    );
                }
            }

            dataFacade.MongoDb.RssHistory.Add(new RssHistoryEntity() {
                ChatId = request.ChatId,
                ThreadId = request.ThreadId,
                RssUrl = request.RssUrl,
                Title = latestArticle.Title,
                Url = latestArticle.Link,
                Author = latestArticle.Author,
                PublishDate = latestArticle.PublishingDate.GetValueOrDefault(DateTime.Now),
                Status = (int)EventStatus.Complete
            });
        }
        catch (Exception exception)
        {
            if (exception.IsRssBetterDisabled())
            {
                var findRssSetting = await dataFacade.MongoDb.RssSetting
                    .Where(entity => entity.ChatId == request.ChatId)
                    .Where(entity => entity.RssUrl == request.RssUrl)
                    .Where(entity => entity.Status == (int)EventStatus.Complete)
                    .ToListAsync(cancellationToken);

                var exceptionMessage = exception.InnerException?.Message ?? exception.Message;

                findRssSetting.ForEach(rssSetting => {
                    logger.LogWarning("Removing RSS CronJob for ChatId: {ChatId}, Url: {Url}. Reason: {Message}",
                        rssSetting.ChatId, rssSetting.RssUrl, exceptionMessage);

                    rssSetting.Status = (int)EventStatus.InProgress;
                    rssSetting.LastErrorMessage = exceptionMessage;

                    serviceFacade.RecurringJobManager.RemoveIfExists(rssSetting.CronJobId);
                });
            }
            else
            {
                logger.LogError(exception, "Error while sending RSS article to Chat: {ChatId}. Url: {Url}",
                    request.ChatId, request.RssUrl);
            }
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return true;
    }
}