using MongoFramework.Linq;
using ZiziBot.Contracts.Enums;

namespace ZiziBot.Console.ViewModels;

public class AppSettingsViewModel(MongoDbContextBase mongoDbContextBase) : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator? Activator { get; }

    [Reactive]
    public IEnumerable<AppSettingsEntity>? AppSettings { get; set; }

    public async Task LoadData()
    {
        AppSettings = await mongoDbContextBase.AppSettings.AsNoTracking()
            .Where(w => w.Status == (int)EventStatus.Complete)
            .OrderBy(o => o.Field).ToListAsync();
    }

    public async Task Update(AppSettingsEntity appSettingsEntity)
    {
        mongoDbContextBase.AppSettings.Update(appSettingsEntity);
        await mongoDbContextBase.SaveChangesAsync();

        await LoadData();
    }
}