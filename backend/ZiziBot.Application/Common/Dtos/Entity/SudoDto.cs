namespace ZiziBot.Application.Common.Dtos.Entity;

public class SudoDto
{
    public long UserId { get; set; }
    public long PromotedBy { get; set; }
    public long PromotedFrom { get; set; }
}