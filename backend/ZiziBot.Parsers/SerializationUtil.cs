using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace ZiziBot.Parsers;

public static class SerializationUtil
{
    public static string ToJson<T>(this T source, bool indented = false)
    {
        return JsonConvert.SerializeObject(source, indented ? Formatting.Indented : Formatting.None);
    }

    public static T? ToObject<T>(this string? source)
    {
        if (string.IsNullOrEmpty(source))
            return default;

        return JsonConvert.DeserializeObject<T>(source);
    }

    public static string ToYaml(this object? obj)
    {
        var serializer = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        var yaml = serializer.Serialize(obj)
            .Trim();

        return yaml;
    }


    public static string? GetTransactionId(this HttpContext context)
    {
        return context.TraceIdentifier;
    }

    public static long GetUserId(this IHeaderDictionary headerDictionary)
    {
        return long.TryParse(headerDictionary[HeaderKey.UserId], out var userId) ? userId : default;
    }

    public static List<long>? GetListChatId(this IHeaderDictionary headerDictionary)
    {
        var listChatId = headerDictionary[HeaderKey.ListChatId].ToString().ToObject<List<long>>();

        return listChatId;
    }

    public static string ToHeaderRawKv(this IHeaderDictionary headerDictionary)
    {
        return headerDictionary.Select(kv => $"{kv.Key}: {kv.Value}").StrJoin("\n");
    }

    public static async Task<string> GetBodyAsync(this HttpContext? context)
    {
        if (context == null) return string.Empty;

        using var reader = new StreamReader(context.Request.Body);

        return await reader.ReadToEndAsync();
    }
}