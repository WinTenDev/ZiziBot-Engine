using Newtonsoft.Json;
using ZiziBot.Application.Common.Converters.Newtonsoft;

namespace ZiziBot.Application.Infrastructure.Vendor.FathimahApi.v1;

public class CityResponse
{
    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("query")]
    public Query Query { get; set; }

    [JsonProperty("kota")]
    public List<City> Cities { get; set; }
}

public class City
{
    [JsonProperty("id")]
    [JsonConverter(typeof(StringToIntConverter))]
    public int Id { get; set; }

    [JsonProperty("nama")]
    public string Name { get; set; }
}