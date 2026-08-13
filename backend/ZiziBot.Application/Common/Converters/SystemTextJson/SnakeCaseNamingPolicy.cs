using System.Text.Json;

namespace ZiziBot.Application.Common.Converters.SystemTextJson;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        return ToSnakeCase(name);
    }

    private static string ToSnakeCase(string str)
    {
        return string.Concat(str.Select((x, i) =>
                i > 0 && char.IsUpper(x)
                    ? "_" + x.ToString()
                    : x.ToString()))
            .ToLower();
    }
}