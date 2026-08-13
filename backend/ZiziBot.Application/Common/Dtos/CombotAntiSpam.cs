using Newtonsoft.Json;

namespace ZiziBot.Application.Common.Dtos;

public class CombotAntispamApiDto
{
    [JsonProperty("ok")]
    public bool Ok { get; set; }
    public bool IsBanned => Result != null;

    [JsonProperty("result")]
    public CombotAntispamResult? Result { get; set; }
}

public class CombotAntispamResult
{
    [JsonProperty("offenses")]
    public long Offenses { get; set; }

    [JsonProperty("messages")]
    public List<string> Messages { get; set; }

    [JsonProperty("time_added")]
    public DateTimeOffset TimeAdded { get; set; }
}