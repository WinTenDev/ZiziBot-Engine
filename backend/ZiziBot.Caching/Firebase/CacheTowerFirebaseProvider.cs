using CacheTower;
using Firebase.Database;
using Firebase.Database.Query;
using Google.Apis.Auth.OAuth2;

namespace ZiziBot.Caching.Firebase;

public class CacheTowerFirebaseProvider(FirebaseCacheOptions cacheOptions) : ICacheLayer
{
    public ValueTask FlushAsync()
    {
        GetClient()
            .Child(cacheOptions.RootDir)
            .DeleteAsync();

        return ValueTask.CompletedTask;
    }

    public ValueTask CleanupAsync()
    {
        GetClient()
            .Child(cacheOptions.RootDir)
            .DeleteAsync();

        return ValueTask.CompletedTask;

    }

    public async ValueTask EvictAsync(string cacheKey)
    {
        await GetClient()
            .Child(cacheOptions.RootDir)
            .Child(cacheKey)
            .DeleteAsync();
    }

    public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
    {
        var cacheEntry = default(CacheEntry<T>?);

        var data = await GetClient()
            .Child(cacheOptions.RootDir)
            .Child(cacheKey)
            .OnceAsync<FirebaseCacheEntry<T>>();

        var obj = data.FirstOrDefault(o => o.Object.CacheKey == cacheKey);

        if (obj?.Object != null)
            return new CacheEntry<T>((T)obj.Object.Value, obj.Object.Expiry);

        return cacheEntry;
    }

    public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
    {
        await GetClient()
            .Child(cacheOptions.RootDir)
            .Child(cacheKey)
            .PutAsync(
                new FirebaseCacheEntry<T?>()
                {
                    CacheKey = cacheKey,
                    Value = cacheEntry.Value,
                    Expiry = cacheEntry.Expiry
                }
            );
    }

    public async ValueTask<bool> IsAvailableAsync(string cacheKey)
    {
        var obj = await GetClient()
            .Child(cacheOptions.RootDir)
            .Child(cacheKey)
            .OnceAsync<object>();

        return obj.Any();
    }

    private FirebaseClient GetClient()
    {
        var client = new FirebaseClient(
            baseUrl: cacheOptions.ProjectUrl,
            options: new FirebaseOptions
            {
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