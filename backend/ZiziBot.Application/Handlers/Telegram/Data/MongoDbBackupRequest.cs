using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.DataSource.MongoDb.Entities;
using File = System.IO.File;

namespace ZiziBot.Application.Handlers.Telegram.Data;

public class MongoDbBackupRequest : IRequest<bool>
{ }

public class MongoDbBackupHandler(
    ILogger<MongoDbBackupHandler> logger,
    DataFacade dataFacade
)
    : IRequestHandler<MongoDbBackupRequest, bool>
{
    public async Task<bool> Handle(MongoDbBackupRequest request, CancellationToken cancellationToken)
    {
        var botMain = await dataFacade.AppSetting.GetBotMain();

        var config = await dataFacade.AppSetting.GetConfigSectionAsync<EventLogConfig>();

        if (config == null)
        {
            logger.LogWarning("Event Log seem not configured yet");
            return false;
        }

        var exportPath = PathConst.MONGODB_BACKUP.EnsureDirectory();

        await dataFacade.MongoDb.ExportAllAsync<AppSettingsEntity>();
        await dataFacade.MongoDb.ExportAllAsync<ApiKeyEntity>();
        await dataFacade.MongoDb.ExportAllAsync<BotCommandEntity>();
        await dataFacade.MongoDb.ExportAllAsync<BotSettingsEntity>();
        await dataFacade.MongoDb.ExportAllAsync<BangHasan_ShalatCityEntity>();
        await dataFacade.MongoDb.ExportAllAsync<ChannelMapEntity>();
        await dataFacade.MongoDb.ExportAllAsync<ChannelPostEntity>();
        // await dataFacade.MongoDb.ExportAllAsync<BinderByteCheckAwbEntity>();
        await dataFacade.MongoDb.ExportAllAsync<ChatSettingEntity>();
        await dataFacade.MongoDb.ExportAllAsync<GlobalBanEntity>();
        await dataFacade.MongoDb.ExportAllAsync<GroupTopicEntity>();
        await dataFacade.MongoDb.ExportAllAsync<MirrorApprovalEntity>();
        await dataFacade.MongoDb.ExportAllAsync<MirrorUserEntity>();
        await dataFacade.MongoDb.ExportAllAsync<NoteEntity>();
        await dataFacade.MongoDb.ExportAllAsync<RssSettingEntity>();
        await dataFacade.MongoDb.ExportAllAsync<SudoerEntity>();
        // await dataFacade.MongoDb.ExportAllAsync<TonjooAwbEntity>();
        await dataFacade.MongoDb.ExportAllAsync<WebhookChatEntity>();
        await dataFacade.MongoDb.ExportAllAsync<WelcomeMessageDto>();
        await dataFacade.MongoDb.ExportAllAsync<WordFilterEntity>();

        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var zipFile = exportPath.CompressToZip($"MongoDB-{date}.zip");

        var htmlMessage = HtmlMessage.Empty;

        var bot = new TelegramBotClient(botMain.Token);
        htmlMessage.Bold("Date: ").CodeBr(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss zzz"));

        await using var fileStream = File.OpenRead(path: zipFile);
        var inputOnlineFile = InputFile.FromStream(stream: fileStream, fileName: zipFile.GetFileName());

        await bot.SendDocumentAsync(
            chatId: config.ChatId,
            document: inputOnlineFile,
            caption: htmlMessage.ToString(),
            parseMode: ParseMode.Html,
            messageThreadId: config.BackupDB,
            cancellationToken: cancellationToken
        );

        PathConst.BACKUP
            .GetFiles(pattern: "MongoDB*zip", predicate: x => DateTime.UtcNow.AddMonths(-2) > x.CreationTime)
            .DeleteFile();

        return true;
    }
}