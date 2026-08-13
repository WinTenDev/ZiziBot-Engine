using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZiziBot.Application.Common.Converters.SystemTextJson;

public class UnixTimestampConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var type = reader.TokenType;
        switch (type)
        {
            case JsonTokenType.String:
                return DateTimeOffset.Parse(reader.GetString() ?? string.Empty);
            case JsonTokenType.Number:
                var unixTimestamp = reader.GetInt64();
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                return dateTime;
            default:
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        var unixTimestamp = new DateTimeOffset(value.UtcDateTime).ToUnixTimeSeconds();
        writer.WriteNumberValue(unixTimestamp);
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class UnixTimestampAttribute : JsonConverterAttribute
{
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        return new UnixTimestampConverter();
    }
}