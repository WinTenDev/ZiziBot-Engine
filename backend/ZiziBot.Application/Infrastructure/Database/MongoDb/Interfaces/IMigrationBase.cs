using JetBrains.Annotations;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Interfaces;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IMigrationBase
{
    Task UpAsync();
    Task DownAsync();
}
