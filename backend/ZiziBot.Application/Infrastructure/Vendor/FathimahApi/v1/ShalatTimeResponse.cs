using Newtonsoft.Json;
using ZiziBot.Application.Common.Converters.Newtonsoft;
using ZiziBot.Application.Common.Enums;
using ZiziBot.Application.Common.Utils;

namespace ZiziBot.Application.Infrastructure.Vendor.FathimahApi.v1;

public class ShalatTimeResponse
{
    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("query")]
    public Query Query { get; set; }

    [JsonProperty("jadwal")]
    public Schedule Schedule { get; set; }
}

public class Schedule
{
    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("data")]
    public Shalat Shalat { get; set; }

    [JsonIgnore]
    public Dictionary<string,string> ShalatDict => Shalat
        .ToDictionary(StringType.TitleCase)
        .Where(pair => pair.Key != "Tanggal")
        .ToDictionary(pair => pair.Key,pair => pair.Value.ToString() ?? string.Empty);
}

public class Shalat
{
    [JsonProperty("ashar")]
    public string Ashar { get; set; }

    [JsonProperty("dhuha")]
    public string Dhuha { get; set; }

    [JsonProperty("dzuhur")]
    public string Dzuhur { get; set; }

    [JsonProperty("imsak")]
    public string Imsak { get; set; }

    [JsonProperty("isya")]
    public string Isya { get; set; }

    [JsonProperty("maghrib")]
    public string Maghrib { get; set; }

    [JsonProperty("subuh")]
    public string Subuh { get; set; }

    [JsonProperty("tanggal")]
    public string Tanggal { get; set; }

    [JsonProperty("terbit")]
    public string Terbit { get; set; }
}

public class Query
{
    [JsonProperty("format")]
    public string Format { get; set; }

    [JsonProperty("kota")]
    [JsonConverter(typeof(StringToIntConverter))]
    public int City { get; set; }

    [JsonProperty("tanggal")]
    public DateTimeOffset Date { get; set; }
}