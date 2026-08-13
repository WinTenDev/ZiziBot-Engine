namespace ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

public class UserOtpEntity : EntityBase
{
    public long UserId { get; set; }
    public int Otp { get; set; }
    public bool IsPermanent { get; set; }
}
