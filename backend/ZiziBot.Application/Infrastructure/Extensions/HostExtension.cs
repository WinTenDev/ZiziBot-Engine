using Microsoft.AspNetCore.Hosting;

namespace ZiziBot.Application.Infrastructure.Extensions;

public static class HostExtension
{
    public static IWebHostBuilder ConfigureCustomListenPort(this IWebHostBuilder webHostBuilder)
    {
        var portVar = EnvUtil.GetEnv("PORT");
        if (portVar?.Length > 0 && int.TryParse(portVar, out int port))
        {
            webHostBuilder.ConfigureKestrel(options => {
                options.ListenAnyIP(port);
            });
        }

        return webHostBuilder;
    }
}