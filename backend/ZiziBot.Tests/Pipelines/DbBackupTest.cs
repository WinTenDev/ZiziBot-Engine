using FluentAssertions;
using Xunit;

namespace ZiziBot.Tests.Pipelines;

public class DbBackupTest(MediatorService mediatorService, AppSettingRepository appSettingRepository)
{
    private readonly AppSettingRepository _appSettingRepository = appSettingRepository;

    [Fact]
    public async Task ExportMongoDbTest()
    {
        var result = await mediatorService.Send(new MongoDbBackupRequest());

        result.Should().BeTrue();
    }
}