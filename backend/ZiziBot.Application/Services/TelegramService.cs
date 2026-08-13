using System.Diagnostics;
using AsyncAwaitBestPractices;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using ZiziBot.Application.Features.Hangfire;
using ZiziBot.Application.Features.Handlers.Telegram.Core;
using ZiziBot.Application.Features.UseCases.Chat;
using ZiziBot.Application.Common.Dtos;
using ZiziBot.Application.Common.Types;
using CreateChatActivityRequest = ZiziBot.Application.Features.UseCases.Chat.CreateChatActivityRequest;
using File = System.IO.File;

namespace ZiziBot.Application.Services;

public class TelegramService(
    ILogger<TelegramService> logger,
    CacheService cacheService,
    MediatorService mediatorService,
    DataFacade dataFacade
)
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private BotRequestBase _request = new();
    private string _timeInit = string.Empty;

    public ITelegramBotClient Bot { get; set; }
    public Message? SentMessage { get; set; }
    private BotResponseBase BotResponse { get; set; } = new();

    public void SetupResponse(BotRequestBase request)
    {
        _request = request.EnsureNotNull();

        _timeInit = request.MessageDate.GetDelay();
        Bot = new TelegramBotClient(request.BotToken);

        if (_request.ReplyMessage)
            _request.ReplyToMessageId = _request.Message?.MessageId ?? 0;
    }

    public async Task<bool> IsBotName(string name)
    {
        var botSettings = await dataFacade.MongoDb.BotSettings
            .FirstOrDefaultAsync(x => x.Token == _request.BotToken);

        return botSettings?.Name == name;
    }

    private string GetExecStamp()
    {
        var timeProc = _request.MessageDate.GetDelay();
        var stamp = $"⏳ <code>{_timeInit} s</code> | ⏱ <code>{timeProc} s</code>";
        return stamp;
    }

    public async Task<string> DownloadFileAsync(string? prefixName = null, string? fileId = null, string? customFileName = null)
    {
        if (fileId.IsNullOrWhiteSpace())
        {
            fileId = (_request.ReplyToMessage ?? _request.Message)?.GetFileId();
            if (fileId.IsNullOrEmpty())
            {
                logger.LogWarning("No file detected in Message: {MessageId}", _request.MessageId);
            }
        }

        var fileName = (_request.ReplyToMessage ?? _request.Message)?.GetFileName();

        if (customFileName.IsNotNullOrWhiteSpace())
            fileName = customFileName;

        var filePath = PathConst.TEMP_PATH + prefixName + fileName;

        fileId.EnsureNotNullOrWhiteSpace();

        await using Stream fileStream = File.OpenWrite(filePath.EnsureDirectory());
        await Bot.GetInfoAndDownloadFile(fileId, fileStream);

        return filePath;
    }

    public BotResponseBase Complete()
    {
        _stopwatch.Stop();

        BotResponse.ChatId = _request.ChatId;
        BotResponse.SentMessage = SentMessage;
        BotResponse.ResponseSource = ResponseSource.Bot;
        BotResponse.ExecutionTime = _stopwatch.Elapsed;

        return BotResponse;
    }

    #region Response
    public async Task<BotResponseBase> SendMessageText(
        HtmlMessage text,
        ReplyMarkup? replyMarkup = null,
        long chatId = -1
    )
    {
        return await SendMessageText(text.ToString(), replyMarkup, chatId);
    }

    public async Task<BotResponseBase> SendMessageText(
        string? text,
        ReplyMarkup? replyMarkup = null,
        long chatId = -1,
        int threadId = -1,
        TimeSpan deleteAfter = default
    )
    {
        var replyParameters = new ReplyParameters() {
            AllowSendingWithoutReply = true
        };

        if (text.IsNullOrEmpty())
            return Complete();

        text += "\n\n" + GetExecStamp();

        var targetChatId = chatId != -1 ? chatId : _request.ChatId;

        if (threadId == -1)
            threadId = _request.MessageThreadId;

        replyParameters.MessageId = _request.ReplyMessage ? _request.ReplyToMessageId : -1;

        if (_request.ReplyToMessage != null)
        {
            replyParameters.MessageId = _request.ReplyToMessage.MessageId;
        }

        logger.LogDebug("Sending message to chat {ChatId}", targetChatId);
        try
        {
            SentMessage = await Bot.SendMessage(
                chatId: targetChatId,
                messageThreadId: threadId,
                text: text,
                replyParameters: replyParameters,
                parseMode: ParseMode.Html,
                replyMarkup: replyMarkup,
                linkPreviewOptions: true
            );
        }
        catch (Exception exception1)
        {
            if (exception1.Message.Contains("thread not found"))
            {
                try
                {
                    logger.LogWarning(exception1, "Trying send message without thread to ChatId: {ChatId}", targetChatId);
                    SentMessage = await Bot.SendMessage(
                        chatId: targetChatId,
                        text: text,
                        replyParameters: _request.ReplyMessage ? _request.ReplyToMessageId : -1,
                        parseMode: ParseMode.Html,
                        replyMarkup: replyMarkup,
                        linkPreviewOptions: true
                    );
                }
                catch (Exception exception2)
                {
                    logger.LogError(exception2, "Error when Sending message to {ChatId}", targetChatId);
                }
            }
        }

        if (SentMessage == null)
            return Complete();

        HangfireUtil.Enqueue<CreateChatActivityUseCase>(x => x.Handle(new CreateChatActivityRequest {
            ActivityType = ChatActivityType.BotSendMessage,
            ChatId = targetChatId.Identifier.GetValueOrDefault(),
            ThreadId = threadId,
            MessageId = SentMessage.MessageId,
            TransactionId = _request.TransactionId
        }));

        logger.LogInformation("Message sent to chat {ChatId}", targetChatId);

        var deleteAfterExec = deleteAfter != TimeSpan.Zero ? deleteAfter : _request.DeleteAfter;

        if (_request.CleanupTargets.Contains(CleanupTarget.None) || deleteAfterExec == TimeSpan.Zero)
            return Complete();

        logger.LogDebug("Schedule delete message {MessageId} on ChatId: {ChatId} in {DeleteAfter} seconds", SentMessage.MessageId, targetChatId, _request.DeleteAfter.TotalSeconds);

        mediatorService.Schedule(new DeleteMessageBotRequestModel {
            BotToken = _request.BotToken,
            Message = _request.Message,
            MessageId = SentMessage.MessageId,
            Source = ResponseSource.Hangfire
        }, deleteAfterExec);

        if (_request.CleanupTargets.Contains(CleanupTarget.FromSender))
        {
            mediatorService.Schedule(new DeleteMessageBotRequestModel {
                BotToken = _request.BotToken,
                Message = _request.Message,
                MessageId = _request.MessageId,
                DeleteAfter = _request.DeleteAfter,
                Source = ResponseSource.Hangfire
            }, deleteAfterExec);
        }

        logger.LogInformation("Message {MessageId} scheduled for deletion in {DeleteAfter} seconds", SentMessage.MessageId, _request.DeleteAfter.TotalSeconds);

        return Complete();
    }

    public async Task<BotResponseBase> EditMessageText(string text, InlineKeyboardMarkup? replyMarkup = null)
    {
        if (SentMessage == null)
            return Complete();

        text += "\n\n" + GetExecStamp();

        await Bot.EditMessageText(_request.ChatId, SentMessage.MessageId, text, replyMarkup: replyMarkup, parseMode: ParseMode.Html);

        HangfireUtil.Enqueue<CreateChatActivityUseCase>(x => x.Handle(new CreateChatActivityRequest {
            ActivityType = ChatActivityType.BotEditMessage,
            ChatId = _request.ChatId.Identifier.GetValueOrDefault(),
            MessageId = SentMessage.MessageId,
            TransactionId = _request.TransactionId
        }));

        return Complete();
    }

    public async Task<BotResponseBase> SendMediaAsync(
        string fileId,
        CommonMediaType? mediaType,
        string? caption = null,
        ReplyMarkup? replyMarkup = null,
        long customChatId = -1,
        string? customFileName = null,
        int? threadId = null
    )
    {
        var targetChatId = customChatId == -1 ? _request.ChatId : customChatId;

        var targetThreadId = threadId ?? _request.MessageThreadId;

        logger.LogInformation(message: "Sending media: {MediaType}, fileId: {FileId} to {ChatId}", mediaType, fileId, targetChatId);

        InputFile inputFile = InputFile.FromFileId(fileId: fileId);

        switch (mediaType)
        {
            case CommonMediaType.Document:

                if (fileId.IsValidUrl())
                {
                    logger.LogInformation(message: "Converting URL: '{Url}' to stream", args: fileId);
                    var stream = await fileId.GetStreamAsync();

                    inputFile = InputFile.FromStream(stream: stream, fileName: customFileName);
                }

                SentMessage = await Bot.SendDocument(
                    chatId: targetChatId,
                    document: inputFile,
                    caption: caption,
                    parseMode: ParseMode.Html,
                    replyMarkup: replyMarkup,
                    replyParameters: _request.ReplyToMessageId,
                    messageThreadId: targetThreadId
                );

                break;

            case CommonMediaType.LocalDocument:
                var fileName = Path.GetFileName(path: fileId);

                await using (var fileStream = File.OpenRead(path: fileId))
                {
                    var inputOnlineFile = InputFile.FromStream(stream: fileStream, fileName: fileName);

                    SentMessage = await Bot.SendDocument(
                        chatId: targetChatId,
                        document: inputOnlineFile,
                        caption: caption,
                        parseMode: ParseMode.Html,
                        replyMarkup: replyMarkup,
                        replyParameters: _request.ReplyToMessageId,
                        messageThreadId: targetThreadId
                    );
                }

                break;

            case CommonMediaType.Photo:
                SentMessage = await Bot.SendPhoto(
                    chatId: targetChatId,
                    photo: inputFile,
                    caption: caption,
                    parseMode: ParseMode.Html,
                    replyMarkup: replyMarkup,
                    replyParameters: _request.ReplyToMessageId,
                    messageThreadId: targetThreadId
                );

                break;

            case CommonMediaType.Audio:
                SentMessage = await Bot.SendAudio(
                    chatId: targetChatId,
                    audio: inputFile,
                    caption: caption,
                    parseMode: ParseMode.Html,
                    replyMarkup: replyMarkup,
                    replyParameters: _request.ReplyToMessageId,
                    messageThreadId: targetThreadId
                );

                break;

            case CommonMediaType.Video:
                SentMessage = await Bot.SendVideo(
                    chatId: targetChatId,
                    video: inputFile,
                    caption: caption,
                    parseMode: ParseMode.Html,
                    replyMarkup: replyMarkup,
                    replyParameters: _request.ReplyToMessageId,
                    messageThreadId: targetThreadId
                );

                break;

            case CommonMediaType.Sticker:
                SentMessage = await Bot.SendSticker(
                    chatId: targetChatId,
                    sticker: inputFile,
                    replyMarkup: replyMarkup,
                    replyParameters: _request.ReplyToMessageId,
                    messageThreadId: targetThreadId
                );

                break;

            case CommonMediaType.Unknown:
            case CommonMediaType.Text:
                await SendMessageText(text: caption, replyMarkup: replyMarkup);
                break;

            default:
                logger.LogWarning(message: "Media unknown: {MediaType}", args: mediaType);
                return null;
        }

        return Complete();
    }

    public async Task<BotResponseBase> EditMediaAsync(
        string fileId,
        CommonMediaType mediaType,
        string? caption = null,
        ReplyMarkup? replyMarkup = null,
        long customChatId = -1,
        string? customFileName = null,
        int? threadId = null,
        int? messageId = null
    )
    {
        var targetChatId = customChatId == -1 ? _request.ChatId : customChatId;

        var targetThreadId = threadId ?? _request.MessageThreadId;
        var targetMessageId = messageId ?? SentMessage.MessageId;

        await Task.Delay(1);

        logger.LogInformation("Sending media: {MediaType}, fileId: {FileId} to {ChatId}", mediaType, fileId,
            targetChatId);

        logger.LogDebug("Updating media caption in {ChatId}:{ThreadId}:{MessageId}", targetChatId, targetThreadId,
            targetMessageId);

        Bot.EditMessageCaption(
            targetChatId,
            targetMessageId,
            caption,
            ParseMode.Html,
            replyMarkup: replyMarkup as InlineKeyboardMarkup
        ).SafeFireAndForget(e => logger.LogWarning(e, "Error updating media caption"));

        InputMedia media = mediaType switch {
            CommonMediaType.Photo => new InputMediaPhoto(new InputFileId(fileId)),
            CommonMediaType.Audio => new InputMediaAudio(new InputFileId(fileId)),
            CommonMediaType.Video => new InputMediaVideo(new InputFileId(fileId)),
            CommonMediaType.Document => new InputMediaDocument(new InputFileId(fileId)),
            _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null)
        };

        logger.LogDebug("Updating media file in {ChatId}:{ThreadId}:{MessageId}", targetChatId, targetThreadId,
            targetMessageId);

        Bot.EditMessageMedia(
            targetChatId,
            targetMessageId,
            media,
            replyMarkup as InlineKeyboardMarkup
        ).SafeFireAndForget(e => logger.LogWarning(e, "Error updating media file"));

        return Complete();
    }

    public async Task<BotResponseBase> SendMessageAsync(
        string? text,
        InlineKeyboardMarkup? replyMarkup = null,
        string? fileId = null,
        CommonMediaType? mediaType = CommonMediaType.Unknown,
        long customChatId = -1,
        int threadId = -1,
        int customMessageId = -1,
        string? customFileName = null,
        TimeSpan deleteAfter = default
    )
    {
        if (SentMessage != null)
        {
            text.EnsureNotNullOrWhiteSpace();
            await EditMessageText(text, replyMarkup);
        }
        else
        {
            if (fileId.IsNullOrEmpty() ||
                mediaType == CommonMediaType.Text ||
                mediaType == CommonMediaType.Unknown)
            {
                await SendMessageText(
                    text,
                    replyMarkup,
                    customChatId,
                    threadId,
                    deleteAfter
                );
            }
            else
            {
                await SendMediaAsync(
                    fileId,
                    mediaType,
                    text,
                    replyMarkup,
                    customChatId,
                    customFileName
                );
            }
        }

        return Complete();
    }

    public async Task DeleteMessageAsync()
    {
        await Bot.DeleteMessage(_request.ChatId, _request.MessageId);
    }

    public async Task<BotResponseBase> AnswerCallbackAsync(string message, bool showAlert = false)
    {
        await Bot.AnswerCallbackQuery(_request.CallbackQueryId, message, showAlert);
        return Complete();
    }

    public async Task<BotResponseBase> AnswerInlineQueryAsync(IEnumerable<InlineQueryResult> results)
    {
        try
        {
            if (_request.InlineQuery == null)
            {
                return Complete();
            }

            var reducedResults = results.Take(50);
            await Bot.AnswerInlineQuery(_request.InlineQuery.Id, reducedResults, 60);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error answering inline query: {InlineQueryId}", _request.InlineQuery?.Id);
        }

        return Complete();
    }

    public async Task LeaveChatAsync()
    {
        await Bot.LeaveChat(_request.ChatId);
    }
    #endregion


    #region Command
    public string? GetCommand(bool withoutSlash = false, bool withoutUsername = true)
    {
        var cmd = string.Empty;

        if (!_request.MessageText?.StartsWith('/') ?? true) return cmd;

        cmd = _request.MessageTexts?.ElementAtOrDefault(0);

        if (withoutSlash)
            cmd = cmd?.TrimStart('/');

        if (withoutUsername)
            cmd = cmd?.Split("@").FirstOrDefault();

        return cmd;
    }

    public bool IsCommand(string command)
    {
        return GetCommand() == command;
    }
    #endregion

    #region Chat
    public async Task<Chat?> GetChatAsync(long chatId)
    {
        try
        {
            var channel = await Bot.GetChat(chatId);

            return channel;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task PinChatMessageAsync(int messageId)
    {
        await Bot.UnpinChatMessage(_request.ChatId, messageId);
        await Bot.PinChatMessage(_request.ChatId, messageId);
    }
    #endregion

    #region Member
    public async Task<int> GetMemberCount()
    {
        var memberCount = await Bot.GetChatMemberCount(_request.ChatId);
        return memberCount;
    }

    public async Task MuteMemberAsync(long userId, TimeSpan duration)
    {
        var untilDate = DateTime.UtcNow + duration;
        await Bot.RestrictChatMember(_request.ChatId, userId, new() {
            CanSendAudios = false,
            CanSendPhotos = false,
            CanSendVideos = false,
            CanSendVideoNotes = false,
            CanSendVoiceNotes = false,
            CanSendDocuments = false
        }, untilDate: untilDate);
    }

    public async Task KickMember(long userId = 0)
    {
        if (userId == 0) userId = _request.UserId;

        await Bot.BanChatMember(_request.ChatId, userId);
        await Bot.UnbanChatMember(_request.ChatId, userId);
    }

    public async Task AnswerJoinRequestAsync(ChatJoinRequest joinRequest)
    {
        await Bot.ApproveChatJoinRequest(_request.ChatId, joinRequest.From.Id);
    }

    public async Task<string[]> GetChatUsernames()
    {
        var cache = await cacheService.GetOrSetAsync(
            CacheKey.CHAT_ACTIVE_USERNAMES + _request.ChatId,
            async () => {
                var chat = await Bot.GetChat(_request.ChatId);
                var activeUsernames = chat.ActiveUsernames;

                return activeUsernames ?? Array.Empty<string>();
            });

        return cache;
    }

    public async Task<string[]> GetUserUsernames()
    {
        var cache = await cacheService.GetOrSetAsync(
            CacheKey.USER_ACTIVE_USERNAMES + _request.UserId,
            async () => {
                var chat = await Bot.GetChat(_request.UserId);
                var activeUsernames = chat.ActiveUsernames;

                return activeUsernames ?? Array.Empty<string>();
            });

        return cache;
    }

    public async Task<string[]> GetAllUsernames()
    {
        var chat = await GetChatUsernames();
        var chatUser = await GetUserUsernames();
        var activeUsernames = chat.Concat(chatUser).ToArray();

        return activeUsernames;
    }

    public async Task<bool> UserHasUsername()
    {
        if (_request.User?.Username != null)
            return true;

        var usernames = await GetUserUsernames();

        return usernames.Any();
    }
    #endregion

    #region Role
    public async Task PromoteMember(long userId)
    {
        logger.LogInformation("Promoting user {UserId} in chat {ChatId}", userId, _request.ChatId);

        await Bot.PromoteChatMember(
            _request.ChatId,
            _request.UserId,
            canPostMessages: true,
            canEditMessages: true,
            canDeleteMessages: true,
            canInviteUsers: true,
            canRestrictMembers: true,
            canPromoteMembers: true,
            canManageVideoChats: true,
            canPinMessages: true
        );
    }

    public async Task DemoteMember(long userId)
    {
        logger.LogInformation("Demoting user {UserId} in chat {ChatId}", userId, _request.ChatId);

        await Bot.PromoteChatMember(
            chatId: _request.ChatId,
            userId: _request.UserId,
            canPostMessages: false,
            canEditMessages: false,
            canDeleteMessages: false,
            canInviteUsers: false,
            canRestrictMembers: false,
            canPromoteMembers: false,
            canManageVideoChats: false,
            canPinMessages: false
        );
    }

    public async Task<List<ChatAdminDto>> GetChatAdministrator()
    {
        if (_request.IsPrivateChat ||
            _request.ChatIdentifier <= 0)
            return [];

        var cacheValue = await cacheService.GetOrSetAsync(
            CacheKey.CHAT_ADMIN + _request.ChatId,
            async () => {
                var chatAdmins = await Bot.GetChatAdministrators(_request.ChatId);
                return chatAdmins.Select(chatMember => {
                    var dto = new ChatAdminDto {
                        User = chatMember.User,
                        Status = chatMember.Status
                    };

                    switch (chatMember)
                    {
                        case ChatMemberOwner owner:
                            dto.CustomTitle = owner.CustomTitle;
                            dto.IsAnonymous = owner.IsAnonymous;
                            break;
                        case ChatMemberAdministrator admin:
                            dto.CustomTitle = admin.CustomTitle;
                            dto.IsAnonymous = admin.IsAnonymous;
                            break;
                    }

                    return dto;
                }).ToList();
            }
        );

        return cacheValue;
    }

    public async Task<bool> CheckAdministration()
    {
        var chatAdmins = await GetChatAdministrator();
        var isAdmin = chatAdmins.Exists(x => x.User.Id == _request.UserId);
        return isAdmin;
    }

    public async Task<bool> CheckChatAdminOrPrivate()
    {
        if (_request.ChatType == ChatType.Private)
            return true;

        return await CheckAdministration();
    }

    public async Task<bool> CheckChatCreator()
    {
        var chatAdmins = await GetChatAdministrator();
        var isAdmin = chatAdmins.Any(x => x.User.Id == _request.UserId && x.Status == ChatMemberStatus.Creator);
        return isAdmin;
    }

    public async Task<bool> CheckBotAdmin()
    {
        var me = await Bot.GetMe();
        var chatAdmins = await GetChatAdministrator();
        return chatAdmins.Any(x => x.User.Id == me.Id);
    }

    private async Task GetRoles()
    {
        _request.RolesLevels.Add(RoleLevel.Guest);
        _request.RolesLevels.Add(RoleLevel.None);

        if (await dataFacade.ChatSetting.IsSudoAsync(_request.UserId))
        {
            _request.RolesLevels.Add(RoleLevel.Sudo);
        }

        if (_request.ChatType == ChatType.Private)
        {
            _request.RolesLevels.Add(RoleLevel.Private);
        }

        if (_request.InlineQuery != null) return;

        if (await CheckChatCreator())
        {
            _request.RolesLevels.Add(RoleLevel.ChatCreator);
        }

        if (await CheckAdministration())
        {
            _request.RolesLevels.Add(RoleLevel.ChatAdmin);
        }
    }

    public async Task<bool> ValidateRole()
    {
        await GetRoles();

        logger.LogDebug("Roles for UserId: {UserId} in ChatId: {ChatId} is: {@Roles}", _request.UserId, _request.ChatId, _request.RolesLevels);

        var isRoleMeet = _request.RolesLevels.Exists(x => x == _request.MinimumRole);
        if (_request.MinimumRole == RoleLevel.ChatAdminOrPrivate)
        {
            isRoleMeet = _request.RolesLevels.Exists(x => x is RoleLevel.ChatCreator or RoleLevel.ChatAdmin or RoleLevel.Private);
        }

        return isRoleMeet;
    }
    #endregion
}
