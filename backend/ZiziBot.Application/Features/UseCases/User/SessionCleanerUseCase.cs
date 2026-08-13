using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.UseCases.User;

public class SessionCleanerUseCase(DataFacade dataFacade)
{
    public async Task Handle()
    {
        var sessions = await dataFacade.MongoDb.DashboardSessions
            .Where(x => x.ExpireDate <= DateTime.UtcNow)
            .ToListAsync();

        sessions.ForEach(x => {
            dataFacade.MongoDb.DashboardSessions.Remove(x);
        });

        await dataFacade.MongoDb.SaveChangesAsync();
    }
}