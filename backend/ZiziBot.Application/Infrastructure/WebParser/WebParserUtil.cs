using PickAll;
using ZiziBot.Application.Common.Types;
using ZiziBot.Application.Common.Utils;

namespace ZiziBot.Application.Infrastructure.WebParser;

public static class WebParserUtil
{
    public static async Task<IEnumerable<WebSearch>> WebSearchText(string search)
    {
        var ctx = await new SearchContext()
            .WithEvents()
            .With<PickAll.Google>()
            .With<Uniqueness>()
            .SearchAsync(search);

        return ctx.Select(x => new WebSearch() {
            Title = x.Description,
            Url = x.Url.UrlDecode()
        });
    }

    public static async Task<string?> HtmlForTelegram(this string? htmlString)
    {
        if (htmlString.IsNullOrEmpty()) return htmlString;

        using var doc = await htmlString.OpenHtml();

        foreach (var element in doc!.QuerySelectorAll("img, svg, body, head"))
        {
            element.Remove();
        }

        htmlString = doc.DocumentElement.OuterHtml;

        var htmlCleaned = htmlString.Replace("<br>", "").Replace("</br>", "")
            .Replace("<p>", "\n").Replace("</p>", "")
            .Replace("<ul>", "").Replace("</ul>", "")
            .Replace("<li>", "- ").Replace("</li>", "")
            .RegexReplace("<h.|<.h.>", "")
            .Trim();

        return htmlCleaned;
    }
}