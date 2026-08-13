using System.ComponentModel;

namespace ZiziBot.Application.Infrastructure.Config;

[DisplayName("BinderByte")]
public class BinderByteConfig
{
    public bool IsEnabled { get; set; }
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
}