﻿using Microsoft.Extensions.Logging;
using MongoFramework.Linq;
using Telegram.Bot.Types;

namespace ZiziBot.Application.Handlers.Telegram.Group;

public class NewChatMembersBotRequest : BotRequestBase
{
    public required User[] NewUser { get; set; }
}

[UsedImplicitly]
public class NewChatMembersHandler : IRequestHandler<NewChatMembersBotRequest, BotResponseBase>
{
    private readonly ILogger<NewChatMembersHandler> _logger;
    private readonly MongoDbContextBase _mongoDbContext;
    private readonly TelegramService _telegramService;
    private ChatSettingRepository _chatSettingRepository;

    public NewChatMembersHandler(
        ILogger<NewChatMembersHandler> logger,
        MongoDbContextBase mongoDbContext,
        TelegramService telegramService,
        ChatSettingRepository chatSettingRepository
    )
    {
        _logger = logger;
        _mongoDbContext = mongoDbContext;
        _telegramService = telegramService;
        _chatSettingRepository = chatSettingRepository;
    }

    public async Task<BotResponseBase> Handle(NewChatMembersBotRequest request, CancellationToken cancellationToken)
    {
        _telegramService.SetupResponse(request);
        _logger.LogInformation("New Chat Members. ChatId: {ChatId}", request.ChatId);

        await _chatSettingRepository.CreateActivity(new ChatActivityDto
        {
            ChatId = request.ChatIdentifier,
            ActivityType = ChatActivityType.NewChatMember,
            Chat = request.Chat,
            User = request.User
        });

        var chatTitle = request.ChatTitle;
        var newMemberCount = request.NewUser.Length;
        var allNewMember = request.NewUser.Select(user => user.GetFullMention()).Aggregate((s, next) => s + ", " + next);
        var greet = TimeUtil.GetTimeGreet();
        var memberCount = await _telegramService.GetMemberCount();

        var messageTemplate = "Hai {AllNewMember}\n" +
                              "Selamat datang di Kontrakan {ChatTitle}";

        var welcomeMessage = await _mongoDbContext.WelcomeMessage.AsNoTracking()
            .Where(x => x.ChatId == request.ChatIdentifier)
            .Where(x => x.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken);

        if (welcomeMessage != null)
        {
            messageTemplate = welcomeMessage.Text;
        }

        var messageText = messageTemplate.ResolveVariable(new List<(string placeholder, string value)>()
        {
            ("AllNewMember", allNewMember),
            // ("AllNoUsername", allNoUsername),
            // ("AllNewBot", allNewBot),
            ("ChatTitle", chatTitle),
            ("Greet", greet),
            ("NewMemberCount", newMemberCount.ToString()),
            ("MemberCount", memberCount.ToString())
        });

        return await _telegramService.SendMessageAsync(
            text: messageText,
            replyMarkup: welcomeMessage?.RawButton.ToButtonMarkup(),
            fileId: welcomeMessage?.Media,
            mediaType: (CommonMediaType)welcomeMessage?.DataType
        );
    }
}