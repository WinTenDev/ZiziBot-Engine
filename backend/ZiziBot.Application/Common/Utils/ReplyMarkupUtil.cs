using Serilog;
using Telegram.Bot.Types.ReplyMarkups;
using ZiziBot.Application.Common.Types;

namespace ZiziBot.Application.Common.Utils;

public static class ReplyMarkupUtil
{
    public static Dictionary<string, string> StringToDict(string buttonStr)
    {
        var dict = new Dictionary<string, string>();
        var splitWelcomeButton = buttonStr.Split(',').ToList();

        foreach (var button in splitWelcomeButton)
        {
            Log.Information("Button: {Button}", button);

            if (button.Contains("|"))
            {
                var buttonLink = button.Split('|').ToList();
                Log.Information(
                    "Appending keyboard: {V} -> {V1}",
                    buttonLink[0],
                    buttonLink[1]
                );
                dict.Add(buttonLink[0], buttonLink[1]);
            }
        }

        return dict;
    }

    public static InlineKeyboardMarkup CreateInlineKeyboardButton(
        Dictionary<string, string> buttonList,
        int columns
    )
    {
        var rows = (int)Math.Ceiling(buttonList.Count / (double)columns);
        var buttons = new InlineKeyboardButton[rows][];

        for (var i = 0; i < buttons.Length; i++)
        {
            buttons[i] = buttonList
                .Skip(i * columns)
                .Take(columns)
                .Select
                (
                    direction => {
                        // if (direction.Value.CheckUrlValid())
                        return InlineKeyboardButton.WithUrl(direction.Key, direction.Value);
                        // else
                        // return InlineKeyboardButton.WithCallbackData(direction.Key, direction.Value);
                    }
                )
                .ToArray();
        }

        return new InlineKeyboardMarkup(buttons);
    }

    public static InlineKeyboardMarkup ToReplyMarkup(
        this string buttonStr,
        int columns = 2
    )
    {
        return CreateInlineKeyboardButton(StringToDict(buttonStr), columns);
    }

    public static List<List<TelegramButtonUrl>> ToListButton(this string? rawButtonMarkups)
    {
        if (string.IsNullOrEmpty(rawButtonMarkups)) return new List<List<TelegramButtonUrl>>();

        var listButtonMapRaw = rawButtonMarkups.Split("\n")
            .Select(
                s => s
                    .Split(",")
                    .Select(
                        x => {
                            var btn = x.Split("|");

                            return new TelegramButtonUrl() {
                                Text = btn.ElementAtOrDefault(0)?.Trim(),
                                Url = btn.ElementAtOrDefault(1)?.Trim()
                            };
                        }
                    ).Where(
                        button =>
                            !string.IsNullOrEmpty(button.Text) &&
                            !string.IsNullOrEmpty(button.Url)
                    ).ToList()
            ).ToList();

        if (listButtonMapRaw.Count == 1)
        {
            listButtonMapRaw = listButtonMapRaw.SelectMany(list => list)
                .Chunk(2)
                .Select(x => x.ToList())
                .ToList();
        }

        return listButtonMapRaw;
    }

    public static IEnumerable<IEnumerable<InlineKeyboardButton>> ToInlineKeyboardButton(
        this IEnumerable<IEnumerable<TelegramButtonUrl>> lisButtonMap,
        bool validateButton = false
    )
    {
        var inlineKeyboardMarkup =
            lisButtonMap
                .Select(
                    buttonRow => buttonRow
                        .Select(
                            buttonCol => {
                                var url = buttonCol.Url;
                                if (!validateButton)
                                    return InlineKeyboardButton.WithUrl(buttonCol.Text, buttonCol.Url);

                                // if (url.IsExistUrl().WaitAndUnwrapException())
                                //     return InlineKeyboardButton.WithUrl(buttonCol.Text, buttonCol.Url);

                                return InlineKeyboardButton.WithCallbackData(buttonCol.Text, $"invalid-url {url}");
                            }
                        )
                );

        return inlineKeyboardMarkup;
    }

    public static IEnumerable<IEnumerable<InlineKeyboardButton>> ToInlineKeyboardButton(
        this string rawButtonMarkups,
        bool validateButton = false
    )
    {
        return rawButtonMarkups.ToListButton().ToInlineKeyboardButton(validateButton: validateButton);
    }

    public static InlineKeyboardMarkup ToButtonMarkup(this string? buttonRaw, bool validateButton = false)
    {
        return buttonRaw.ToListButton().ToInlineKeyboardButton(validateButton: validateButton).ToButtonMarkup();
    }

    public static InlineKeyboardMarkup ToButtonMarkup(this IEnumerable<IEnumerable<TelegramButtonUrl>> listButton)
    {
        return new InlineKeyboardMarkup(listButton.ToInlineKeyboardButton());
    }

    public static InlineKeyboardMarkup ToButtonMarkup(
        this IEnumerable<IEnumerable<InlineKeyboardButton>> listKeyboardButton)
    {
        return new InlineKeyboardMarkup(listKeyboardButton);
    }
}