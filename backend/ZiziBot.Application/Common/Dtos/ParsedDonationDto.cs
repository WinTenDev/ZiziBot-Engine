using ZiziBot.Application.Common.Enums;

namespace ZiziBot.Application.Common.Dtos;

public class ParsedDonationDto
{
    public ParseMethod Method { get; set; }
    public bool IsValid { get; set; }
    public DonationSource Source { get; set; }
    public string? OrderId { get; set; }
    public string PaymentUrl { get; set; }
    public DateTime OrderDate { get; set; }
    public string? PaymentMethod { get; set; }
    public int CendolCount => Subtotal / 5000;
    public string? Cendols { get; set; }
    public int AdminFees { get; set; }
    public int Subtotal { get; set; }
    public string? RawText { get; set; }
}

public enum ParseMethod
{
    Unknown,
    InternalTrakteer,
    TrakteerApi,
    InternalSaweria,
    SaweriaApi,
    WebHookTrakteer
}

public class TrakteerApiDto
{
    public bool IsValid { get; set; }
    public string? OrderId { get; set; }
    public string PaymentUrl { get; set; }
    public int CendolCount { get; set; }
    public int AdminFees { get; set; }
    public int Total { get; set; }
    public DateTime OrderDate { get; set; }
    public string? RawText { get; set; }
    public string? PaymentMethod { get; set; }
}