using Xunit;

namespace ZiziBot.Tests.Services;

public class SubdlRestServiceTests(SubdlRestService subdlRestService)
{
    [Fact]
    public async Task FetchPopularTest()
    {
        // arrange
        var result = await subdlRestService.FetchPopular();

        // assert
        result.Results.ShouldNotBeNull().ShouldNotBeEmpty();
    }
}