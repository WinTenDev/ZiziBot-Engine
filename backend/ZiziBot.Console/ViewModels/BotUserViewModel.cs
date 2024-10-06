using MongoFramework.Linq;
using ZiziBot.Contracts.Enums;

namespace ZiziBot.Console.ViewModels;

public class BotUserViewModel(MongoDbContextBase mongoDbContextBase) : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }

    [Reactive]
    public IEnumerable<BotUserEntity> BotUsers { get; private set; }

    public async Task GetUsers()
    {
        BotUsers = await mongoDbContextBase.BotUser.AsNoTracking()
                                            .Where(x => x.Status == (int)EventStatus.Complete)
                                            .ToListAsync();
    }
}