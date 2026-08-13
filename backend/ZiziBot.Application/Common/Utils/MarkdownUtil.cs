using Markdig;

namespace ZiziBot.Application.Common.Utils;

public static class MarkdownUtil
{
    public static string MdToHtml(this string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var html = Markdown.ToHtml(markdown, pipeline)
            .Replace("<p>", "").Replace("</p>", "");

        return html;
    }
}