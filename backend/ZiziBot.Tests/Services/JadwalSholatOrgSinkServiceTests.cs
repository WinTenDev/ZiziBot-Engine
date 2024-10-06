using FluentAssertions;
using MongoFramework.Linq;
using MoreLinq;
using SharpX.Extensions;
using Xunit;

namespace ZiziBot.Tests.Services;

public class JadwalSholatOrgSinkServiceTests(JadwalSholatOrgSinkService jadwalSholatOrgSinkService, MongoDbContextBase mongoDbContextBase)
{
    [Fact(Skip = "Deprecated")]
    public async Task FeedCityTest()
    {
        mongoDbContextBase.JadwalSholatOrg_City.RemoveRange(entity => true);
        await mongoDbContextBase.SaveChangesAsync();

        await jadwalSholatOrgSinkService.FeedCity();
    }

    [Fact(Skip = "Deprecated")]
    public async Task FeedScheduleTest()
    {
        var cities = await mongoDbContextBase.JadwalSholatOrg_City.ToListAsync();

        var randomCities = cities.Shuffle().Take(3);

        await randomCities.ForEachAsync(async entity => await jadwalSholatOrgSinkService.FeedSchedule(entity.CityId));
    }

    [Theory(Skip = "Deprecated")]
    [InlineData(124)]
    public async Task FeedSchedule_ByCity_Test(int cityId)
    {
        var feedSchedule = await jadwalSholatOrgSinkService.FeedSchedule(cityId);

        feedSchedule.Should().BeGreaterThan(0, because: "Expected FeedSchedule to insert at least one schedule for the given cityId");
    }
}