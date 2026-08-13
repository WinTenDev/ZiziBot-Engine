namespace ZiziBot.Application.Common.Dtos;

public class AntiSpamDto
{
    public bool IsBanAny => IsBanEss || IsBanCasFed || IsBanSwFed;
    public bool IsBanEss { get; set; }
    public bool IsBanCasFed { get; set; }
    public bool IsBanSwFed { get; set; }

    public CombotAntispamResult? CasRecord { get; set; }
    public SpamwatchDto? SpamWatchRecord { get; set; }
}