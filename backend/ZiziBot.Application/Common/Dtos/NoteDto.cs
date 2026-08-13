using Newtonsoft.Json;
using ZiziBot.Application.Common.Enums;

namespace ZiziBot.Application.Common.Dtos;

public class NoteDto
{
    public string Id { get; set; }
    public long ChatId { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? ChatTitle { get; set; }

    public string Query { get; set; }
    public string? Text { get; set; }
    public string? RawButton { get; set; }
    public string? FileId { get; set; }

    public int DataType { get; set; }
    public string DataTypeName => ((CommonMediaType)DataType).ToString();

    public int Status { get; set; }
    public string StatusName => ((EventStatus)Status).ToString();

    public string TransactionId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}