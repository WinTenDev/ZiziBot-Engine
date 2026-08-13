namespace ZiziBot.Application.Common.Dtos.Entity;

public class ChatRestrictionDto : EntityDtoBase
{
    public long UserId { get; set; }
    public long ChatId { get; set; }
}