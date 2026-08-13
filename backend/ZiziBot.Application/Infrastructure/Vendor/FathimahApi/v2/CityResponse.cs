using System.Text.Json.Serialization;

namespace ZiziBot.Application.Infrastructure.Vendor.FathimahApi.v2;

public class CityResponse
{
    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("request")]
    public RequestData Request { get; set; }

    [JsonPropertyName("data")]
    public CityData[] Cities { get; set; }
}