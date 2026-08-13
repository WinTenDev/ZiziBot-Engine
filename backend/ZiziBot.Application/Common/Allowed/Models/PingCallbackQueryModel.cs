using System.Text.Json.Serialization;

namespace ZiziBot.Application.Common.Allowed.Models;

public class PingCallbackQueryModel
{
    [JsonPropertyName("webhookInfo")]
    public string Data { get; set; }
}