using Newtonsoft.Json;

namespace ZiziBot.Application.Common.Dtos;

public class SpamWatchResult
{
    public bool IsBanned { get; set; }
    public SpamwatchDto? BanRecord { get; set; }
}

public class SpamwatchDto
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("admin")]
    public long Admin { get; set; }

    [JsonProperty("date")]
    public long Date { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonProperty("error")]
    public string? Error { get; set; }

    [JsonProperty("reason")]
    public string? Reason { get; set; }

}