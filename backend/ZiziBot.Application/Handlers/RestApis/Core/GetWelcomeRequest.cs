using Microsoft.Extensions.Hosting;

namespace ZiziBot.Application.Handlers.RestApis.Core;

public class GetWelcomeRequest : ApiRequestBase<GetWelcomeResponse>
{
}

public class GetWelcomeResponse
{
    public string AppName { get; set; }
    public string AppVersion { get; set; }
    public string VersionNumber { get; set; }
    public string BuildDate { get; set; }
    public string Environment { get; set; }
}

public class GetWelcomeHandler(IHostEnvironment hostEnvironment) : IRequestHandler<GetWelcomeRequest, ApiResponseBase<GetWelcomeResponse>>
{
    public async Task<ApiResponseBase<GetWelcomeResponse>> Handle(GetWelcomeRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<GetWelcomeResponse>();

        await Task.Delay(1, cancellationToken);

        return response.Success("Welcome to ZiziBot", new GetWelcomeResponse
        {
            AppName = "ZiziBot 5",
            AppVersion = VersionUtil.GetVersion(true),
            VersionNumber = VersionUtil.GetVersion(),
            BuildDate = VersionUtil.GetBuildDate().ToString("yyyy-MM-dd HH:mm:ss zzz"),
            Environment = hostEnvironment.EnvironmentName
        });
    }
}