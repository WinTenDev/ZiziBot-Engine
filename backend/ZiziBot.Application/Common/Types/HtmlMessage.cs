using System.Net;
using System.Text;
using Telegram.Bot.Types;
using ZiziBot.Application.Common.Utils;

namespace ZiziBot.Application.Common.Types;

// Source: https://raw.githubusercontent.com/AleXr64/Telegram-bot-framework/master/TGBotFramework/BotFramework/Utils/HtmlString.cs

public class HtmlMessage
{
    private readonly StringBuilder _stringBuilder = new();

    public static HtmlMessage Empty => new();

    public HtmlMessage Bold(string text) => TagBuilder("b", text);
    public HtmlMessage Bold(HtmlMessage inner) => TagBuilder("b", inner);
    public HtmlMessage BoldBr(string text) => TagBuilder("b", text).Br();

    public HtmlMessage Italic(string text) => TagBuilder("i", text);
    public HtmlMessage Italic(HtmlMessage inner) => TagBuilder("i", inner);

    public HtmlMessage Underline(string text) => TagBuilder("u", text);
    public HtmlMessage Underline(HtmlMessage inner) => TagBuilder("u", inner);

    public HtmlMessage Strike(string text) => TagBuilder("s", text);
    public HtmlMessage Strike(HtmlMessage inner) => TagBuilder("s", inner);

    public HtmlMessage Url(string url, string text) => UrlTagBuilder("a", $"href=\"{url}\"", text);
    public HtmlMessage User(long id, string text) => Url($"tg://user?id={id}", text);
    public HtmlMessage User(User? user) => user == null ? this : User(user.Id, user.GetFullName());

    public HtmlMessage Text(string? text, bool encoded = false)
    {
        _stringBuilder.Append(encoded ? WebUtility.HtmlEncode(text) : text);

        return this;
    }

    public HtmlMessage TextBr(string text, bool encoded = false) => Text(text + System.Environment.NewLine, encoded);

    public HtmlMessage Br()
    {
        _stringBuilder.Append(System.Environment.NewLine);
        return this;
    }

    public HtmlMessage Append(HtmlMessage message)
    {
        _stringBuilder.Append(message);
        return this;
    }

    public HtmlMessage UserMention(User user)
    {
        var fullName = (user.FirstName + " " + user.LastName).Trim();
        var name = fullName.Length > 0 ? fullName : user.Username;
        return User(user.Id, name ?? string.Empty);
    }

    public HtmlMessage Code(string text) => TagBuilder("code", text);
    public HtmlMessage CodeBr(string text) => TagBuilder("code", text).Br();

    public HtmlMessage Pre(string text) => TagBuilder("pre", text);

    public HtmlMessage CodeWithStyle(string style, string text)
    {
        var str = WebUtility.HtmlEncode(text);
        _stringBuilder.Append($"<pre><code class=\"{style}\">");
        _stringBuilder.Append(str);
        _stringBuilder.Append("</code></pre>");
        return this;
    }

    private HtmlMessage TagBuilder(string tag, string text)
    {
        var str = WebUtility.HtmlEncode(text);
        _stringBuilder.Append($"<{tag}>");
        _stringBuilder.Append(str);
        _stringBuilder.Append($"</{tag}>");
        return this;
    }

    private HtmlMessage TagBuilder(string tag, HtmlMessage innerSting)
    {
        _stringBuilder.Append($"<{tag}>");
        _stringBuilder.Append(innerSting);
        _stringBuilder.Append($"</{tag}>");
        return this;
    }

    private HtmlMessage UrlTagBuilder(string tag, string tagParams, string text)
    {
        var str = WebUtility.HtmlEncode(text);
        _stringBuilder.Append($"<{tag} {tagParams}>");
        _stringBuilder.Append(str);
        _stringBuilder.Append($"</{tag}>");
        return this;
    }

    public HtmlMessage PopLine(int count = 1)
    {
        var sbStr = _stringBuilder.ToString();

        _stringBuilder.Clear();

        sbStr.Split(System.Environment.NewLine)
            .SkipLast(count)
            .ToList()
            .ForEach(s => TextBr(s.Trim()));

        return this;
    }

    public override string ToString() => _stringBuilder.ToString().Trim();
    public bool IsEmpty => _stringBuilder.ToString().IsNullOrEmpty();
}