using Xunit;

namespace ZiziBot.Tests.Services;

public class FirebaseTests(FirebaseClientService firebaseClientService) : IAsyncLifetime
{
    [Fact]
    public async Task FirebasePostTest()
    {
        var data = new
        {
            Name = "Fulan"
        };

        var path = "test/post/" + Guid.NewGuid();

        await firebaseClientService.PostAsync(path, data);
        true.ShouldBeTrue();
    }

    [Fact]
    public async Task FirebasePutTest()
    {
        var data = new
        {
            Name = "Fulan",
            Oid = Guid.NewGuid()
        };

        var path = "test/put";

        await firebaseClientService.PutAsync(path, data);
        true.ShouldBeTrue();
    }


    public async ValueTask DisposeAsync()
    {
        await firebaseClientService.DeleteAsync("test");
    }

    public ValueTask InitializeAsync()
    {
        throw new NotImplementedException();
    }
}