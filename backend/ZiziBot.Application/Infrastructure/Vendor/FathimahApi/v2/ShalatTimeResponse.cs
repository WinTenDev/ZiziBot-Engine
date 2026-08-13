using System.Text.Json.Serialization;
using ZiziBot.Application.Common.Enums;
using ZiziBot.Application.Common.Utils;

namespace ZiziBot.Application.Infrastructure.Vendor.FathimahApi.v2;

public class ShalatTimeResponse
{
    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("request")]
    public RequestData Request { get; set; }

    [JsonPropertyName("data")]
    public CityData? Schedule { get; set; }
}

public class RequestData
{
    [JsonPropertyName("path")]
    public string Path { get; set; }
}

public class CityData
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("lokasi")]
    public string Lokasi { get; set; }

    [JsonPropertyName("daerah")]
    public string Daerah { get; set; }

    [JsonPropertyName("jadwal")]
    public Jadwal Jadwal { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public Dictionary<string,string> ShalatDict => Jadwal
        .ToDictionary(StringType.TitleCase)
        .Where(pair => pair.Key != "Tanggal" & pair.Key != "Date")
        .ToDictionary(pair => pair.Key,pair => pair.Value.ToString() ?? string.Empty);
}

public class Jadwal
{
    [JsonPropertyName("tanggal")]
    public string Tanggal { get; set; }

    [JsonPropertyName("imsak")]
    public string Imsak { get; set; }

    [JsonPropertyName("subuh")]
    public string Subuh { get; set; }

    [JsonPropertyName("terbit")]
    public string Terbit { get; set; }

    [JsonPropertyName("dhuha")]
    public string Dhuha { get; set; }

    [JsonPropertyName("dzuhur")]
    public string Dzuhur { get; set; }

    [JsonPropertyName("ashar")]
    public string Ashar { get; set; }

    [JsonPropertyName("maghrib")]
    public string Maghrib { get; set; }

    [JsonPropertyName("isya")]
    public string Isya { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; }
}