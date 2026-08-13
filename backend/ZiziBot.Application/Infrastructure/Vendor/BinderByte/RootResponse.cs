using System.Text.Json.Serialization;

namespace ZiziBot.Application.Infrastructure.Vendor.BinderByte;

public class RootResponse
{
    [JsonPropertyName("status")]
    public long Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public Data Data { get; set; }
}

public class Data
{
    [JsonPropertyName("summary")]
    public Summary Summary { get; set; }

    [JsonPropertyName("detail")]
    public Detail Detail { get; set; }

    [JsonPropertyName("history")]
    public List<History> History { get; set; }
}

public class Detail
{
    [JsonPropertyName("origin")]
    public string Origin { get; set; }

    [JsonPropertyName("destination")]
    public string Destination { get; set; }

    [JsonPropertyName("shipper")]
    public string Shipper { get; set; }

    [JsonPropertyName("receiver")]
    public string Receiver { get; set; }
}

public class History
{
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("desc")]
    public string Desc { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }
}

public class Summary
{
    [JsonPropertyName("awb")]
    public string Awb { get; set; }

    [JsonPropertyName("courier")]
    public string Courier { get; set; }

    [JsonPropertyName("service")]
    public string Service { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("desc")]
    public string Desc { get; set; }

    [JsonPropertyName("amount")]
    public string Amount { get; set; }

    [JsonPropertyName("weight")]
    public string Weight { get; set; }
}