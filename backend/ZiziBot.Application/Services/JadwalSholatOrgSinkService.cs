using Microsoft.Extensions.Logging;
using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Services;

public class JadwalSholatOrgSinkService(ILogger<JadwalSholatOrgSinkService> logger, MongoDbContextBase mongoDbContext)
{
    public async Task FeedAll()
    {
        await FeedCity();
        await FeedSchedule();
    }

    public async Task FeedCity()
    {
        var trxId = Guid.NewGuid().ToString();
        var cities = await JadwalSholatOrgParserUtil.GetCities();
        var removeCityIds = cities.Select(x => x.CityId);

        var insertCities = cities?.Select(x => new JadwalSholatOrg_CityEntity() {
            CityId = x.CityId,
            CityCode = x.CityCode,
            CityName = x.CityName,
            Status = (int)EventStatus.Complete,
            TransactionId = trxId
        });

        if (insertCities != null)
        {
            logger.LogDebug("Deleting old cities..");
            mongoDbContext.JadwalSholatOrg_City.RemoveRange(entity => removeCityIds.Contains(entity.CityId));
            await mongoDbContext.SaveChangesAsync();

            logger.LogDebug("Inserting new cities..");
            mongoDbContext.JadwalSholatOrg_City.AddRange(insertCities);
            await mongoDbContext.SaveChangesAsync();
        }
    }

    public async Task FeedSchedule()
    {
        var cities = await mongoDbContext.JadwalSholatOrg_City.ToListAsync();

        foreach (var city in cities)
        {
            await FeedSchedule(city.CityId);
        }
    }


    public async Task<int> FeedSchedule(int cityId)
    {
        var trxId = Guid.NewGuid().ToString();
        var schedules = await JadwalSholatOrgParserUtil.FetchSchedules(cityId);

        var insertSchedules = schedules?.Select(x => new JadwalSholatOrg_ScheduleEntity() {
            CityId = cityId,
            Date = x.Date,
            Fajr = x.Fajr,
            Sunrise = x.Sunrise,
            Dhuhr = x.Dhuhr,
            Ashr = x.Ashr,
            Maghrib = x.Maghrib,
            Isha = x.Isha,
            Status = (int)EventStatus.Complete,
            TransactionId = trxId
        }).ToList();

        if (insertSchedules.IsEmpty())
        {
            logger.LogInformation("No schedules found for city with ID {cityId}.", cityId);
            return default;
        }

        logger.LogDebug("Deleting old schedules for city {cityId}..", cityId);
        mongoDbContext.JadwalSholatOrg_Schedule.RemoveRange(x => x.CityId == cityId);
        await mongoDbContext.SaveChangesAsync();

        logger.LogDebug("Inserting new schedules for city {cityId}..", cityId);
        mongoDbContext.JadwalSholatOrg_Schedule.AddRange(insertSchedules);
        await mongoDbContext.SaveChangesAsync();

        return insertSchedules.Count;
    }
}