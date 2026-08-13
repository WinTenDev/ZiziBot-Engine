using System.ComponentModel;

namespace ZiziBot.Application.Infrastructure.Config;

[DisplayName("Mirror")]
public class MirrorConfig
{
    public long ApprovalChannelId { get; set; }
    public int PaymentExpirationDays { get; set; }
    public bool UseCustomTrakteerApi { get; set; }
    public bool UseCustomSaweriaApi { get; set; }
    public string TrakteerVerificationApi { get; set; }
    public string SaweriaVerificationApi { get; set; }
    public string TrakteerWebHookToken { get; set; }
}