namespace ZiziBot.Application.Common.Dtos;

public class SaweriaParsedDto
{
    public bool IsValid { get; set; }
    public string PaymentUrl { get; set; }
    public int CendolCount { get; set; }
    public int Total { get; set; }
    public DateTime OrderDate { get; set; }
    public string? OrderId { get; set; }
}