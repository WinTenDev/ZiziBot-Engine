using Microsoft.Extensions.Logging;
using MongoFramework.Linq;
using Telegram.Bot.Types;

namespace ZiziBot.Application.Handlers.Telegram.Group;

public class NewChatMembersBotRequest : BotRequestBase
{
    public required User[] NewUser { get; set; }
}

[UsedImplicitly]
public class NewChatMembersHandler(
    ILogger<NewChatMembersHandler> logger,
    DataFacade dataFacade,
    ServiceFacade serviceFacade
)
    : IRequestHandler<NewChatMembersBotRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(NewChatMembersBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);
        logger.LogInformation("New Chat Members. ChatId: {ChatId}", request.ChatId);

        var create = await dataFacade.ChatSetting.MeasureActivity(new ChatActivityDto {
            ChatId = request.ChatIdentifier,
            UserId = request.UserId,
            ActivityType = ChatActivityType.NewChatMember,
            Chat = request.Chat,
            User = request.User,
            Status = EventStatus.Complete,
            TransactionId = request.TransactionId,
            MessageId = request.MessageId
        });

        if (create)
        {
            var message = HtmlMessage.Empty;
            message.Bold("Anti-RAID mode.").Br()
                .Text("Terdeteksi banyak anggota baru masuk ke grub dalam beberapa waktu terakhir. " +
                      "Untuk alasan keamanan, anggota baru yang masuk dalam {COOLDOWN_TIME} akan disenyapkan.").Br();

            await serviceFacade.TelegramService.SendMessageAsync(message.ToString());
            await serviceFacade.TelegramService.MuteMemberAsync(request.UserId, ValueConst.NEW_MEMBER_RAID_MODE_MUTE_DURATION);

            return serviceFacade.TelegramService.Complete();
        }

        var chatTitle = request.ChatTitle;
        var newMemberCount = request.NewUser.Length;
        var allNewMember = request.NewUser.Select(user => user.GetFullMention())
            .Aggregate((s, next) => s + ", " + next);

        var greet = TimeUtil.GetTimeGreet();
        var memberCount = await serviceFacade.TelegramService.GetMemberCount();

        var messageTemplate = "Hai {AllNewMember}\n" +
                              "Selamat datang di Kontrakan {ChatTitle}";

        var welcomeMessage = await dataFacade.MongoDb.WelcomeMessage.AsNoTracking()
            .Where(x => x.ChatId == request.ChatIdentifier)
            .Where(x => x.Status == (int)EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken);

        if (welcomeMessage != null)
        {
            messageTemplate = welcomeMessage.Text;
        }

        var messageText = messageTemplate.ResolveVariable(new List<(string placeholder, string value)>() {
            ("AllNewMember", allNewMember),
            // ("AllNoUsername", allNoUsername),
            // ("AllNewBot", allNewBot),
            ("ChatTitle", chatTitle),
            ("Greet", greet),
            ("NewMemberCount", newMemberCount.ToString()),
            ("MemberCount", memberCount.ToString())
        });

        return await serviceFacade.TelegramService.SendMessageAsync(
            text: messageText,
            replyMarkup: welcomeMessage?.RawButton.ToButtonMarkup(),
            fileId: welcomeMessage?.Media,
            mediaType: (CommonMediaType)welcomeMessage?.DataType
        );
    }
}