using ZiziBot.DataSource.MongoEf;
using ZiziBot.Interfaces;

namespace ZiziBot.Application.Facades;

public class DataFacade(
    ICacheService cacheService,
    MongoDbContextBase mongoDb,
    MongoEfContext mongoEf,
    AdditionalRepository additionalRepository,
    AppSettingRepository appSettingRepository,
    ChatSettingRepository chatSettingRepository,
    GroupRepository groupRepository,
    MirrorUserRepository mirrorUserRepository,
    WordFilterRepository wordFilterRepository
) : IDataFacade
{
    public ICacheService Cache { get; } = cacheService;

    public MongoDbContextBase MongoDb => mongoDb;
    public MongoEfContext MongoEf => mongoEf;

    public AdditionalRepository Additional => additionalRepository;
    public AppSettingRepository AppSetting => appSettingRepository;
    public ChatSettingRepository ChatSetting => chatSettingRepository;
    public GroupRepository Group => groupRepository;
    public MirrorUserRepository MirrorUser => mirrorUserRepository;
    public WordFilterRepository WordFilter => wordFilterRepository;
}