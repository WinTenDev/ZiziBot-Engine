using System.ComponentModel;

namespace ZiziBot.Application.Infrastructure.Config;

[DisplayName("OptiicDev")]
public class OptiicDevConfig
{
    public bool IsEnabled { get; set; }
    public string ApiKey { get; set; }

    public List<string> ApiKeys()
    {
        return ApiKey.Split(",").Select(x => x.Trim()).ToList();
    }
}