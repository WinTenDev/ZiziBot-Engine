using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace ZiziBot.Services;

public class FirebaseService(IOptions<GcpConfig> options)
{
    private GcpConfig GcpConfig => options.Value;

    public async Task PostAsync<T>(string resourceName, T data)
    {
        await GetClient().Child(resourceName).PostAsync(data);
    }

    public async Task PutAsync<T>(string resourceName, T data)
    {
        await GetClient().Child(resourceName).PutAsync(data);
    }

    public async Task DeleteAsync(string resourceName)
    {
        await GetClient().Child(resourceName).DeleteAsync();
    }

    public IObservable<FirebaseEvent<T>> AsObservable<T>(string resourceName)
    {
        return GetClient().Child(resourceName).AsObservable<T>();
    }

    private FirebaseClient GetClient()
    {
        if (!GcpConfig.IsEnabled)
            throw new AppException("Firebase is disabled");

        var client = new FirebaseClient(
            baseUrl: GcpConfig.FirebaseProjectUrl,
            options: new FirebaseOptions
            {
                AuthTokenAsyncFactory = GetAccessToken,
                AsAccessToken = true,
            }
        );

        return client;
    }

    private async Task<string> GetAccessToken()
    {
        var credential = GoogleCredential.FromJson(GcpConfig.FirebaseServiceAccountJson)
            .CreateScoped("https://www.googleapis.com/auth/firebase.database", "https://www.googleapis.com/auth/userinfo.email");

        var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

        return accessToken;
    }
}