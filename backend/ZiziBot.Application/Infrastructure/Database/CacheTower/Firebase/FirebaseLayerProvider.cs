using CacheTower;
using Firebase.Database;
using Firebase.Database.Query;
using Google.Apis.Auth.OAuth2;
using Serilog;

namespace ZiziBot.Application.Infrastructure.Database.CacheTower.Firebase;

internal class FirebaseLayerProvider(FirebaseCacheOptions cacheOptions) : ICacheLayer
{
    private readonly ILogger _log = Log.ForContext<FirebaseLayerProvider>();

    public ValueTask FlushAsync()
    {
        _log.Verbose("Flush CacheTower firebase layer");
        GetClient()
            .Child(cacheOptions.RootDir)
            .DeleteAsync();

        _log.Verbose("Flush CacheTower firebase layer has done");

        return ValueTask.CompletedTask;
    }

    public ValueTask CleanupAsync()
    {
        _log.Verbose("Cleanup CacheTower firebase layer");
        GetClient()
            .Child(cacheOptions.RootDir)
            .DeleteAsync();

        _log.Verbose("Cleanup CacheTower firebase layer done");

        return ValueTask.CompletedTask;
    }

    public async ValueTask EvictAsync(string cacheKey)
    {
        _log.Verbose("Evict CacheTower firebase layer. Key: {CacheKey}", cacheKey);
        await GetClient()
            .Child(cacheOptions.RootDir)
            .Child(cacheKey)
            .DeleteAsync();

        _log.Verbose("Evict CacheTower firebase layer. Key: {CacheKey}. Done", cacheKey);
    }

    public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
    {
        _log.Verbose("Get CacheTower firebase layer. Key: {CacheKey}", cacheKey);
        var cacheEntry = default(CacheEntry<T>?);

        var obj = await GetClient()
            .Child(cacheOptions.RootDir)
            .Child(cacheKey)
            .OnceSingleAsync<FirebaseCacheEntry<T>>();

        if (obj != null)
        {
            _log.Verbose("Get CacheTower firebase layer. Key: {CacheKey}. Done", cacheKey);
            return new CacheEntry<T>(obj.Value, obj.Expiry);
        }

        return cacheEntry;
    }

    public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
    {
        _log.Verbose("Set CacheTower firebase layer. Key: {CacheKey}", cacheKey);
        await GetClient()
            .Child(cacheOptions.RootDir)
            .Child(cacheKey)
            .PutAsync(
                new FirebaseCacheEntry<T>() {
                    CacheKey = cacheKey,
                    Expiry = cacheEntry.Expiry,
                    Value = cacheEntry.Value,
                }
            );

        _log.Verbose("Set CacheTower firebase layer. Key: {CacheKey}. Done", cacheKey);
    }

    public async ValueTask<bool> IsAvailableAsync(string cacheKey)
    {
        var obj = await GetClient()
            .Child(cacheOptions.RootDir)
            .Child(cacheKey)
            .OnceAsJsonAsync();

        var isAvailable = obj.IsNotNullOrWhiteSpace();
        _log.Verbose("CacheTower Firebase layer is available: {IsAvailable}", isAvailable);

        return isAvailable;
    }

    private FirebaseClient GetClient()
    {
        var client = new FirebaseClient(
            baseUrl: cacheOptions.ProjectUrl,
            options: new FirebaseOptions {
                AuthTokenAsyncFactory = GetAccessToken,
                AsAccessToken = true
            }
        );

        return client;
    }

    private async Task<string> GetAccessToken()
    {
        var credential = GoogleCredential.FromJson(cacheOptions.ServiceAccountJson)
            .CreateScoped("https://www.googleapis.com/auth/firebase.database", "https://www.googleapis.com/auth/userinfo.email");

        var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

        return accessToken;
    }
}

public class FirebaseCacheOptions
{
    public string? ProjectUrl { get; set; }
    public string? ServiceAccountJson { get; set; }
    public string RootDir { get; set; } = "cache";
}
