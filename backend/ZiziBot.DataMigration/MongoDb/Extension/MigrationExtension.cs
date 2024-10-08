﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ZiziBot.Contracts.Constants;
using ZiziBot.DataMigration.MongoDb.Migrations;
using ZiziBot.Utils;

namespace ZiziBot.DataMigration.MongoDb.Extension;

public static class MigrationExtension
{
    public static void AddMongoMigration(this IServiceCollection services)
    {
        var connectionStr = EnvUtil.GetEnv(Env.MONGODB_CONNECTION_STRING);
        var url = new MongoUrl(connectionStr);

        services.AddSingleton<MigrationRunner>();
        services.AddSingleton<IMongoDatabase>(provider => {
            var client = new MongoClient(connectionStr);
            var database = client.GetDatabase(url.DatabaseName);
            return database;
        });

        services.Scan(selector =>
            selector.FromAssembliesOf(typeof(IMigration))
                .AddClasses(filter => filter.InNamespaceOf<IMigration>())
                .As<IMigration>()
                .WithTransientLifetime()
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