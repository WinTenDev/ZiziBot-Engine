namespace ZiziBot.Contracts.Dtos;

public class GlobalBanDto
{
    public long UserId { get; set; }
    public long ChatId { get; set; }
    public string Reason { get; set; }
}