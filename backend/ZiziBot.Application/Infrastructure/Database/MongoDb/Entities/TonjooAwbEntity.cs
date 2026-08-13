using MongoDB.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Vendor.TonjooStudio;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("Tonjoo.Awb")]
public class TonjooAwbEntity : EntityBase
{
    public string Awb { get; set; }
    public string Courier { get; set; }
    public TonjooAwbDetail Detail { get; set; }
}
