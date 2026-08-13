using Jsonize;
using Jsonize.Abstractions.Models;
using Jsonize.Parser;
using Jsonize.Serializer;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;
using YamlDotNet.Serialization;
using ZiziBot.Application.Common.Constants;

namespace ZiziBot.Application.Common.Utils;

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

    public async static Task<JsonizeNode> Jsonize(this string url)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(url);

        var html = await response.Content.ReadAsStringAsync();

        // The use of the parameterless constructors will use default settings.
        var parser = new JsonizeParser();
        var serializer = new JsonizeSerializer();

        var jsonizer = new Jsonizer(parser, serializer);

        var jsonizeNode = await jsonizer.ParseToJsonizeNodeAsync(html);

        return jsonizeNode;
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


    public static string GetTransactionId(this HttpContext context)
    {
        return context.TraceIdentifier;
    }

    public static long GetUserId(this IHeaderDictionary headerDictionary)
    {
        return long.TryParse(headerDictionary[RequestKey.UserId], out var userId) ? userId : default;
    }

    public static List<long>? GetListChatId(this IHeaderDictionary headerDictionary)
    {
        var listChatId = headerDictionary[RequestKey.ListChatId].ToString().ToObject<List<long>>();

        return listChatId;
    }

    public static string ToHeaderRawKv(this IHeaderDictionary headerDictionary)
    {
        return headerDictionary.Select(kv => $"{kv.Key}: {kv.Value}").StrJoin("\n");
    }

    public static async Task<string> GetBodyAsync(this HttpContext? context)
    {
        try
        {
            if (context == null) return string.Empty;

            using var reader = new StreamReader(context.Request.Body);

            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error when reading body");
            throw;
        }
    }
}