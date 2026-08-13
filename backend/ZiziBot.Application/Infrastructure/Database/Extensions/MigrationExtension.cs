using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ZiziBot.Application.Infrastructure.Database.MongoDb;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Interfaces;

namespace ZiziBot.Application.Infrastructure.Database.Extensions;

public static class MigrationExtension
{
    public static void AddMongoMigration(this IServiceCollection services)
    {
        var connectionStr = EnvUtil.GetEnv(Env.MONGODB_CONNECTION_STRING);
        var url = new MongoUrl(connectionStr);

        services.AddSingleton<MigrationRunner>();
        services.AddScoped<IMongoDatabase>(provider => {
            var client = new MongoClient(connectionStr);
            var database = client.GetDatabase(url.DatabaseName);
            return database;
        });

        services.Scan(selector =>
            selector.FromAssembliesOf(typeof(IMigration))
                .AddClasses(filter => filter.AssignableTo<IMigration>())
                .As<IMigration>()
                .WithScopedLifetime()
        );

        services.Scan(selector =>
            selector.FromAssembliesOf(typeof(IPreMigration))
                .AddClasses(filter => filter.AssignableTo<IPreMigration>())
                .As<IPreMigration>()
                .WithScopedLifetime()
        );

        services.Scan(selector =>
            selector.FromAssembliesOf(typeof(IPostMigration))
                .AddClasses(filter => filter.AssignableTo<IPostMigration>())
                .As<IPostMigration>()
                .WithScopedLifetime()
        );
    }

    public static async Task UseMongoMigration(this IApplicationBuilder app)
    {
        var runner = app.ApplicationServices.GetService<MigrationRunner>();

        if (runner == null)
            throw new ApplicationException("Mongo Migration not yet prepared");

        await runner.ApplyMigrationAsync();
    }
}
