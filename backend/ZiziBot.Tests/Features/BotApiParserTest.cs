using AngleSharp.Html.Dom;
using FluentAssertions;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ZiziBot.Tests.Features;

public class BotApiParserTest(ILogger<BotApiParserTest> logger)
{
    [Theory]
    [InlineData("https://core.telegram.org/bots/api")]
    public async Task ParseWebTest(string url)
    {
        var document = await url.OpenUrl();

        document.Should().NotBeNull();
        document.Title.Should().Be("Telegram Bot API");

        var methodList = document.Anchors.Select(element => {
            return new BotApiDoc()
            {
                Href = element.Href,
                Text = element.Parent?.TextContent,
                Fields = document.Anchors
                    .Where(x => {
                        return x.Parent.NextSibling.NextSibling.NextSibling.NextSibling.NodeName == "TABLE";
                    })
                    .Select(anchorElement => {
                        var parent = anchorElement.Parent;
                        var descriptionElement = parent?.NextSibling;
                        var tableElement = descriptionElement?.NextSibling;
                        var t = tableElement.NextSibling;
                        var tablee = t.NextSibling as IHtmlTableElement;

                        return new ApiDocField()
                        {
                            Name = parent?.TextContent,
                            Type = anchorElement.Parent?.TextContent,
                            Description = anchorElement.Parent?.Parent?.TextContent
                        };
                    }).ToList()
            };
        });

        methodList.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("https://raw.githubusercontent.com/PaulSonOfLars/telegram-bot-api-spec/main/api.json")]
    [InlineData("https://raw.githubusercontent.com/PaulSonOfLars/telegram-bot-api-spec/main/api.min.json")]
    public async Task ParseSpec(string url)
    {
        logger.LogInformation("Parsing spec from {url}", url);

        var api = await url.GetJsonAsync<TgBotApiDoc>();

        api.Should().NotBeNull();
    }
}