using MongoDB.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Vendor.BinderByte;

namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

[Collection("BinderByte.CheckAwb")]
public class BinderByteCheckAwbEntity : EntityBase
{
    public string Awb { get; set; }
    public string Courier { get; set; }
    public Data AwbInfo { get; set; }
}
