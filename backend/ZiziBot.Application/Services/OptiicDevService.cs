using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ZiziBot.Application.Services;

public class OptiicDevService(ILogger<OptiicDevService> logger, IOptionsSnapshot<OptiicDevConfig> options)
{
    private OptiicDevConfig OptiicDevConfig => options.Value;

    public async Task<OptiicDevOcrResult> ScanImageAsync(string fileName)
    {
        logger.LogInformation("Scanning image {fileName}", fileName);

        var apiKey = OptiicDevConfig.ApiKeys.RandomPick();

        var response = await UrlConst.OCR_URL_API.PostMultipartAsync(
            content => {
                content.AddString("apiKey", apiKey)
                    .AddFile("image", fileName);
            }
        );

        var result = await response.GetJsonAsync<OptiicDevOcrResult>();

        return result;
    }
}