namespace ZiziBot.Application.Common.Dtos.Entity;

public class EntityDtoBase
{
    public int Status { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedDate { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public long? UpdatedBy { get; set; }
}