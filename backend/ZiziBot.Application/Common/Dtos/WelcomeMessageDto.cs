namespace ZiziBot.Application.Common.Dtos;

public class WelcomeMessageDto
{
    public string Id { get; set; }
    public long ChatId { get; set; }
    public string? ChatTitle { get; set; }
    public string Text { get; set; }
    public string RawButton { get; set; }
    public string Media { get; set; }
    public int DataType { get; set; }
    public string DataTypeName { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; }
}