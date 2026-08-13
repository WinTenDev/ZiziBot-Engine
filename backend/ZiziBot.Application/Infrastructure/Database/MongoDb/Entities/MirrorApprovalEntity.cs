using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("MirrorApproval")]
public class MirrorApprovalEntity : EntityBase
{
    public long UserId { get; set; }
    public DonationSource? DonationSource { get; set; }
    public string? DonationSourceName { get; set; }
    public string? PaymentUrl { get; set; }
    public string? RawText { get; set; }
    public int CendolCount { get; set; }
    public string? Cendols { get; set; }
    public int AdminFees { get; set; }
    public int Subtotal { get; set; }
    public DateTime OrderDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? OrderId { get; set; }
    public string? FileId { get; set; }
    public string? Note { get; set; }
}
