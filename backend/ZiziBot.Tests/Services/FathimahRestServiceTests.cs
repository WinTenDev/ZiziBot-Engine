using Xunit;

namespace ZiziBot.Tests.Services;

public class FathimahRestServiceTests(
    FathimahRestService fathimahRestService
)
{
    public static readonly object[][] ShalatCity =
    [
        [DateTime.Now, 1203],
        // [DateTime.Now.AddMonths(-2).AddDays(10).AddYears(-1), 1203],
        // [new DateTime(2022, 3, 1), 1203]
    ];

    [Fact]
    public async Task GetListCityTest()
    {
        var allCity = await fathimahRestService.GetAllCityAsync();

        allCity.Status.ShouldBe(true);
        allCity.Cities.ShouldNotBeEmpty();
    }

    [Theory, MemberData(nameof(ShalatCity))]
    public async Task GetShalatTimeTest(DateTime dateTime, int cityId)
    {
        var shalatTime = await fathimahRestService.GetShalatTime(dateTime, cityId);

        shalatTime.Status.ShouldBe(true);
        shalatTime.Schedule.ShouldNotBeNull()
            .ShalatDict.ShouldNotBeNull().ShouldNotBeEmpty();
    }
}