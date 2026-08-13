using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Services;

public class JadwalSholatOrgSinkService(ILogger<JadwalSholatOrgSinkService> logger, DataFacade dataFacade)
{
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task FeedAll()
    {
        await FeedCity();
        // await FeedSchedule();
    }

    public async Task FeedCity()
    {
        var trxId = Guid.NewGuid().ToString();
        var cities = await JadwalSholatOrgParserUtil.GetCities();
        if (cities == null)
        {
            logger.LogWarning("No cities found.");
            return;
        }

        var removeCityIds = cities.Select(x => x.CityId).ToList();

        var insertCities = cities.Select(x => new JadwalSholatOrg_CityEntity() {
            CityId = x.CityId,
            CityCode = x.CityCode,
            CityName = x.CityName,
            Status = EventStatus.Complete,
            TransactionId = trxId
        }).ToList();

        if (insertCities.Count != 0)
        {
            logger.LogDebug("Deleting old cities..");
            await dataFacade.MongoDb.JadwalSholatOrg_City.Where(entity => removeCityIds.Contains(entity.CityId)).ExecuteDeleteAsync();
            await dataFacade.MongoDb.SaveChangesAsync();

            logger.LogDebug("Inserting new cities..");
            dataFacade.MongoDb.JadwalSholatOrg_City.AddRange(insertCities);
            await dataFacade.MongoDb.SaveChangesAsync();
        }
    }

    public async Task FeedSchedule()
    {
        var cities = await dataFacade.MongoDb.JadwalSholatOrg_City.ToListAsync();

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
            Status = EventStatus.Complete,
            TransactionId = trxId
        }).ToList();

        if (insertSchedules.IsEmpty())
        {
            logger.LogInformation("No schedules found for city with ID {CityId}.", cityId);
            return default;
        }

        logger.LogDebug("Deleting old schedules for city {CityId}..", cityId);
        var jadwalSholatOrgSchedule = await dataFacade.MongoDb.JadwalSholatOrg_Schedule.Where(x => x.CityId == cityId).ToListAsync();
        dataFacade.MongoDb.RemoveRange(jadwalSholatOrgSchedule);
        await dataFacade.MongoDb.SaveChangesAsync();

        logger.LogDebug("Inserting new schedules for city {CityId}..", cityId);
        dataFacade.MongoDb.JadwalSholatOrg_Schedule.AddRange(insertSchedules);
        await dataFacade.MongoDb.SaveChangesAsync();

        return insertSchedules.Count;
    }
}
