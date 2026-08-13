using Newtonsoft.Json;

namespace ZiziBot.Application.Infrastructure.Vendor.TonjooStudio;

public class CheckAwb
{
    [JsonProperty("data")]
    public Data? Data { get; set; }
}

public class Data
{
    [JsonProperty("found")]
    public bool Found { get; set; }

    [JsonProperty("detail")]
    public TonjooAwbDetail AwbDetail { get; set; }
}

public class TonjooAwbDetail
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("kurir")]
    public List<string> Kurir { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("service")]
    public string Service { get; set; }

    [JsonProperty("origin")]
    public string Origin { get; set; }

    [JsonProperty("destination")]
    public string Destination { get; set; }

    [JsonProperty("shipper")]
    public UserAddress Shipper { get; set; }

    [JsonProperty("consignee")]
    public UserAddress Consignee { get; set; }

    [JsonProperty("date_shipment")]
    public DateTimeOffset? DateShipment { get; set; }

    [JsonProperty("date_received")]
    public DateTimeOffset? DateReceived { get; set; }

    [JsonProperty("receiver")]
    public string Receiver { get; set; }

    [JsonProperty("current_position")]
    public string CurrentPosition { get; set; }

    [JsonProperty("history")]
    public List<History> History { get; set; }
}

public class UserAddress
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; }
}

public class History
{
    [JsonProperty("time")]
    public DateTimeOffset Time { get; set; }

    [JsonProperty("position")]
    public string Position { get; set; }

    [JsonProperty("desc")]
    public string Desc { get; set; }
}