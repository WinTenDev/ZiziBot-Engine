using Hangfire.LiteDB;
using Hangfire.Storage.SQLite;
using MongoDB.Driver;
using Serilog;
using ZiziBot.Application.Common.Exceptions;

namespace ZiziBot.Application.Features.Hangfire
{
    public static class StorageUtil
    {
        internal static MongoStorage ToMongoDbStorage(this string? connectionString)
        {
            var mongoUrlBuilder = new MongoUrlBuilder(connectionString);
            var settings = MongoClientSettings.FromUrl(mongoUrlBuilder.ToMongoUrl());

            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var mongoClient = new MongoClient(settings);

            var databaseName = mongoUrlBuilder.DatabaseName;

            mongoClient.GetDatabase(databaseName);

            try
            {
                var mongoStorage = new MongoStorage(
                    mongoClient: mongoClient,
                    databaseName: databaseName,
                    storageOptions: new MongoStorageOptions()
                    {
                        MigrationOptions = new MongoMigrationOptions
                        {
                            MigrationStrategy = new MigrateMongoMigrationStrategy(),
                            BackupStrategy = new CollectionMongoBackupStrategy()
                        },
                        CheckConnection = false,
                        CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.Poll
                    }
                );

                return mongoStorage;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to create Hangfire MongoDB Storage");

                if (ex is MongoCommandException mongoCommandException)
                {
                    Log.Warning(mongoCommandException, "Resetting MongoDb database {DatabaseName} for hangfire", databaseName);

                    mongoClient.GetDatabase(databaseName)
                        .ListCollectionNames()
                        .ToList()
                        .Where(x => x.StartsWith("hangfire", StringComparison.OrdinalIgnoreCase))
                        .ToList()
                        .ForEach(x => mongoClient.GetDatabase(databaseName).DropCollection(x));
                }

                throw new AppException("Fail to create Hangfire MongoDB Storage. Please restart Engine");
            }
        }

        internal static LiteDbStorage ToLiteDbStorage()
        {
            return new LiteDbStorage(PathConst.HANGFIRE_LITEDB_PATH.EnsureDirectory());
        }

        internal static SQLiteStorage ToSqliteStorage()
        {
            return new SQLiteStorage(PathConst.HANGFIRE_SQLITE_PATH.EnsureDirectory());
        }
    }
}