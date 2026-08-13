using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.Application.Common.Utils;

namespace ZiziBot.Tests.SampleData;

public static class SampleMessages
{
    private static readonly long[] ChatIds = [
        -1001710313973,
        -1001929269823,
        -1001645422053,
        -1001936696407,
        -1001404591750
    ];

    public static Message CommonMessage => new() {
        // MessageId = 316001,
        MessageThreadId = 297986,
        ForumTopicCreated = new ForumTopicCreated() {
            Name = "Sample - Taufik Name"
        },
        ForumTopicEdited = new ForumTopicEdited() {
            Name = "Sample - New Taufik Name"
        },
        Date = DateTime.UtcNow,
        EditDate = DateTime.UtcNow,
        Chat = new Chat {
            Id = ChatIds.RandomPick(),
            Title = "Sample - Grub Name"
        },
        From = new User {
            Id = 236205726,
            FirstName = "Azhe",
            LastName = "Kun",
            Username = "azhe403"
        }
    };

    public static Message NewChatMembers => new() {
        // MessageId = 12345,
        MessageThreadId = 321067,
        Chat = new Chat() {
            Id = -1001404591750,
            Title = "Grub Name"
        },
        From = new User() {
            Id = 552609163,
            FirstName = "I'am",
            LastName = "Groot",
            Username = "groot"
        },
        NewChatMembers = new[] {
            new User() {
                Id = 1025424321,
                FirstName = "Sandal",
                LastName = "Jepit"
            },
            new User() {
                Id = 1025424321,
                FirstName = "Fulan",
                LastName = "Ahmad"
            },
            new User() {
                Id = 552609163,
                FirstName = "I'm",
                LastName = "Groot"
            }
        }
    };

    public static Update UpdateChannelPost => new() {
        Id = 0,
        Message = null,
        EditedMessage = null,
        ChannelPost = ChannelPost,
        EditedChannelPost = null,
    };

    public static Update UpdateChannelPostDocument => new() {
        Id = 0,
        Message = null,
        EditedMessage = null,
        ChannelPost = ChannelPostDocument,
        EditedChannelPost = null,
    };

    public static Update UpdateEditedChannelPost => new() {
        Id = 0,
        Message = null,
        EditedMessage = null,
        ChannelPost = null,
        EditedChannelPost = ChannelPost
    };

    public static Message ChannelPost = new() {
        // MessageId = 45,
        Date = DateTime.UtcNow,
        Chat = new() {
            Id = -1001556283448,
            Type = ChatType.Channel,
            Title = "Sample - Telegram Bot API Channel",
        },
        Text = "this is message text"
    };

    public static Message ChannelPostDocument = new() {
        // MessageId = 45,
        Date = DateTime.UtcNow,
        Chat = new() {
            Id = -1001556283448,
            Type = ChatType.Channel,
            Title = "Sample - Telegram Bot API Channel",
        },
        Caption = "this is message caption",
        Document = new() {
            FileId = "BQACAgUAAx0CU7hehgABBNdfZPrwISnUYnEMrm5Tzpo40aZBdXMAAl0LAAIXHEhVVErnd8J1j8YwBA",
            FileUniqueId = "BQACAgUAA",
            FileSize = 2000,
            FileName = "FileName.exe"
        }
    };
}