using MongoDB.EntityFrameworkCore;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("MirrorDonation")]
public class MirrorDonationEntity : EntityBase
{
    public string OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Type { get; set; }
    public string SupporteName { get; set; }
    public string SupporterAvatar { get; set; }
    public string SupporterMessage { get; set; }
    public string Unit { get; set; }
    public string UnitIcon { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public int NetAmount { get; set; }
    public long ClaimerUserId { get; set; }
    public DonationSource Source { get; set; }
}
