using System.Reflection;
using MongoFramework;
using MongoFramework.Infrastructure;
using MongoFramework.Linq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.DataSource.MongoDb;

public class MongoDbContextBase(string connectionStr) : MongoDbContext(MongoDbConnection.FromConnectionString(connectionStr))
{
    public MongoDbSet<AppSettingsEntity> AppSettings { get; set; }
    public MongoDbSet<BotSettingsEntity> BotSettings { get; set; }
    public MongoDbSet<BotCommandEntity> BotCommand { get; set; }
    public MongoDbSet<SudoerEntity> Sudoers { get; set; }

    public MongoDbSet<FeatureFlagEntity> FeatureFlag { get; set; }
    public MongoDbSet<FeatureRoleEntity> FeatureRole { get; set; }
    public MongoDbSet<FeatureRoleFlagEntity> FeatureRoleFlag { get; set; }
    public MongoDbSet<FeatureAssignmentEntity> FeatureAssignment { get; set; }

    public MongoDbSet<DashboardSessionEntity> DashboardSessions { get; set; }
    public MongoDbSet<ApiKeyEntity> ApiKey { get; set; }
    public MongoDbSet<BotUserEntity> BotUser { get; set; }

    public MongoDbSet<ChannelMapEntity> ChannelMap { get; set; }
    public MongoDbSet<ChannelPostEntity> ChannelPost { get; set; }
    public MongoDbSet<ChatRestrictionEntity> ChatRestriction { get; set; }
    public MongoDbSet<GlobalBanEntity> GlobalBan { get; set; }

    public MongoDbSet<ChatAdminEntity> GroupAdmin { get; set; }
    public MongoDbSet<GroupTopicEntity> GroupTopic { get; set; }
    public MongoDbSet<WelcomeMessageEntity> WelcomeMessage { get; set; }
    public MongoDbSet<AfkEntity> Afk { get; set; }
    public MongoDbSet<WarnUserEntity> WarnUser { get; set; }
    public MongoDbSet<ChatAdminEntity> ChatAdmin { get; set; }

    public MongoDbSet<ChatGptSessionEntity> ChatGptSession { get; set; }
    public MongoDbSet<ChatSettingEntity> ChatSetting { get; set; }
    public MongoDbSet<ChatActivityEntity> ChatActivity { get; set; }
    public MongoDbSet<NoteEntity> Note { get; set; }
    public MongoDbSet<RssSettingEntity> RssSetting { get; set; }
    public MongoDbSet<RssHistoryEntity> RssHistory { get; set; }

    public MongoDbSet<WebhookChatEntity> WebhookChat { get; set; }
    public MongoDbSet<WebhookHistoryEntity> WebhookHistory { get; set; }

    public MongoDbSet<WordFilterEntity> WordFilter { get; set; }

    public MongoDbSet<MirrorUserEntity> MirrorUsers { get; set; }
    public MongoDbSet<MirrorApprovalEntity> MirrorApproval { get; set; }

    public MongoDbSet<BinderByteCheckAwbEntity> BinderByteCheckAwb { get; set; }
    public MongoDbSet<TonjooAwbEntity> TonjooAwb { get; set; }

    public MongoDbSet<BangHasan_ShalatCityEntity> BangHasan_ShalatCity { get; set; }

    public MongoDbSet<JadwalSholatOrg_CityEntity> JadwalSholatOrg_City { get; set; }
    public MongoDbSet<JadwalSholatOrg_ScheduleEntity> JadwalSholatOrg_Schedule { get; set; }

    public override void SaveChanges()
    {
        EnsureTimestamp();
        base.SaveChanges();
    }

    public override Task SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        EnsureTimestamp();
        return base.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> ExportAllAsync<T>() where T : class, new()
    {
        var exportPath = PathConst.MONGODB_BACKUP.EnsureDirectory();
        var data = await Query<T>().ToListAsync();
        var entityName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name!;

        var path = Path.Combine(exportPath, entityName + ".csv").EnsureDirectory();

        data.WriteToCsvFile(path);

        return path;
    }

    private void EnsureTimestamp()
    {
        ChangeTracker.DetectChanges();

        var entries = ChangeTracker
            .Entries()
            .Where(entityEntry =>
                entityEntry.Entity is EntityBase &&
                entityEntry.State is EntityEntryState.Added or EntityEntryState.Updated or EntityEntryState.Deleted
            );

        foreach (var entityEntry in entries)
        {
            ((EntityBase)entityEntry.Entity).UpdatedDate = DateTime.UtcNow;

            switch (entityEntry.State)
            {
                case EntityEntryState.Added:
                    ((EntityBase)entityEntry.Entity).CreatedDate = DateTime.UtcNow;
                    break;

                case EntityEntryState.Deleted:
                    ((EntityBase)entityEntry.Entity).Status = (int)EventStatus.Deleted;
                    break;
            }
        }
    }
}