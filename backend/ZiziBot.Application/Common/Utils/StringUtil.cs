using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using Flurl;
using NanoidDotNet;
using Serilog;
using ZiziBot.Application.Common.Constants;

namespace ZiziBot.Application.Common.Utils;

public static class StringUtil
{
    public static T? GetCommandParamAt<T>(this string? param, int index, string separator = " ")
    {
        return string.IsNullOrEmpty(param) ? default : param.Split(separator, StringSplitOptions.TrimEntries).GetCommandParamAt<T>(index);
    }

    public static bool IsCommand(this string? text, string command)
    {
        return text.GetCommandParamAt<string>(0)?.ToLower().Equals(command.ToLower()) ?? false;
    }

    public static string HtmlEncode(this string html)
    {
        return HttpUtility.HtmlEncode(html);
    }

    public static string HtmlDecode(this string html)
    {
        return HttpUtility.HtmlDecode(html);
    }

    public static string UrlEncode(this string url)
    {
        return HttpUtility.UrlEncode(url);
    }

    public static string UrlDecode(this string url)
    {
        return HttpUtility.UrlDecode(url);
    }

    public static string GetNanoId(int size = 11)
    {
        return Nanoid.Generate(alphabet: Nanoid.Alphabets.LettersAndDigits, size: size);
    }

    public static async Task<string> GetNanoIdAsync(string prefix = "", int size = 11)
    {
        var id = await Nanoid.GenerateAsync(alphabet: Nanoid.Alphabets.LettersAndDigits, size: size);
        return $"{prefix}{id}";
    }

    public static async Task<string> GenerateRssKeyAsync()
    {
        var uniqueId = await GetNanoIdAsync(prefix: $"{CronJobKey.Rss_Prefix}:", size: 7);
        return uniqueId.ToUpper();
    }

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static bool IsNotNullOrEmpty(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }

    public static bool IsNotNullOrWhiteSpace(this string? str)
    {
        return !string.IsNullOrWhiteSpace(str);
    }

    public static bool IsValidGuid(this string guid)
    {
        return Guid.TryParse(guid, out _);
    }

    public static TValue ToEnum<TValue>(this string value, TValue defaultValue) where TValue : struct
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;

        return Enum.TryParse<TValue>(value, true, out var result) ? result : defaultValue;
    }

    public static string ResolveVariable(this string? input, IEnumerable<(string placeholder, string value)> placeHolders)
    {
        return placeHolders.Aggregate(input, (current, ph) => current?.Replace($"{{{ph.placeholder}}}", ph.value, StringComparison.CurrentCultureIgnoreCase)) ?? string.Empty;
    }

    public static string Sha256Hash(string value)
    {
        using SHA256 hash = SHA256.Create();
        return hash.ComputeHash(Encoding.UTF8.GetBytes(value)).Select(i => i.ToString("x2")).Aggregate((a, b) => a + b);
    }

    public static byte[] ShaHash(String value)
    {
        using var hasher = SHA256.Create();
        return hasher.ComputeHash(Encoding.UTF8.GetBytes(value));
    }

    public static byte[] HashHmac(byte[] key, byte[] message)
    {
        var hash = new HMACSHA256(key);
        return hash.ComputeHash(message);
    }

    public static string HashHmac(string key, string message)
    {
        var secretKey = ShaHash(key);

        var hashHmac = HashHmac(secretKey, Encoding.UTF8.GetBytes(message))
            .Select(i => i.ToString("x2"))
            .Aggregate((a, b) => a + b);

        return hashHmac;
    }

    public static string ForCacheKey(this string input)
    {
        if (input.IsValidUrl())
        {
            var urlParse = input.UrlParse();
            var qParam = urlParse.QueryParams.Select((x) => $"{x.Name}={x.Value}").StrJoin("/");
            var prefixed = Url.Combine(urlParse.RemoveQuery(), qParam.IsNotNullOrWhiteSpace() ? Sha256Hash(qParam) : "");

            if (input != prefixed)
                Log.Debug("Convert cache key from {Before} => {After}", input, prefixed);

            input = prefixed;
        }

        var key = input
            .UrlDecode()
            .HtmlDecode()
            .Replace("https://", "")
            .Replace("http://", "")
            .Replace(".", "-")
            .Replace("?", "_")
            .Replace("=", "_")
            .RegexReplaceEval(@"(%20|%22|\s)+", "_")
            .TrimEnd('_')
            .TrimEnd("/");

        if (input != key)
            Log.Debug("Convert cache key from {Before} => {After}", input, key);

        return key;
    }

    public static string StrJoin(this IEnumerable<string> source, string separator = ",")
    {
        return string.Join(separator, source);
    }

    public static string TrimStart(this string source, string? value, StringComparison comparisonType = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (string.IsNullOrEmpty(value))
            return source;

        int valueLength = value.Length;
        int startIndex = 0;

        while (source.IndexOf(value, startIndex, comparisonType) == startIndex)
        {
            startIndex += valueLength;
        }

        return source.Substring(startIndex);
    }

    public static string TrimEnd(this string source, string? value, StringComparison comparisonType = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (string.IsNullOrEmpty(value))
            return source;

        int sourceLength = source.Length;
        int valueLength = value.Length;
        int count = sourceLength;

        while (source.LastIndexOf(value, count, comparisonType) == count - valueLength)
        {
            count -= valueLength;
        }

        return source.Substring(0, count);
    }

    public static string[] Explode(this string? input)
    {
        return input?.Split(ValueConst.WordSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
    }

    public static string RegexReplace(this string input, string pattern, string replacement)
    {
        return Regex.Replace(input, pattern, replacement, RegexOptions.NonBacktracking, matchTimeout: TimeSpan.FromMilliseconds(100));
    }

    public static string RegexReplaceEval(this string input, string pattern, string replacement)
    {
        return Regex.Replace(input, pattern, Eval, RegexOptions.NonBacktracking, matchTimeout: TimeSpan.FromMilliseconds(100));

        string Eval(Match m)
        {
            return replacement;
        }
    }

    public static string RegexMatch(this string input, string pattern)
    {
        return Regex.Match(input, pattern, RegexOptions.NonBacktracking, matchTimeout: TimeSpan.FromMilliseconds(100)).Value;
    }

    public static string RegexMatchIf(this string input, string pattern, bool condition)
    {
        return condition ? input.RegexMatch(pattern) : input;
    }

    public static string ToCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
        {
            return input;
        }

        var chars = input.ToCharArray();
        chars[0] = char.ToLowerInvariant(input[0]);

        return new(chars);
    }

    public static bool Like(this string? input, string what)
    {
        return !input.IsNullOrEmpty() &&
               input.Contains(what, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool Match(this string source, string pattern)
    {
        var isMatch = source == pattern;

        if (pattern.StartsWith('*'))
            if (source.StartsWith(pattern.Replace("*", "")))
                isMatch = true;

        if (pattern.EndsWith('*'))
            if (source.EndsWith(pattern.Replace("*", "")))
                isMatch = true;

        if (pattern.StartsWith('*') && pattern.EndsWith('*'))
            if (source.Contains(pattern.Replace("*", "")))
                isMatch = true;

        if (!pattern.IsValidRegex())
            return isMatch;

        if (Regex.IsMatch(source, pattern, RegexOptions.NonBacktracking, matchTimeout: TimeSpan.FromMilliseconds(100)))
            isMatch = true;

        return isMatch;
    }

    public static bool IsValidRegex(this string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        try
        {
            _ = Regex.Match("dummy-texts", pattern, RegexOptions.NonBacktracking, matchTimeout: TimeSpan.FromMilliseconds(100));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static T? Deserialize<T>(this string input, JsonNamingPolicy? jsonNamingPolicy = null)
    {
        return JsonSerializer.Deserialize<T>(input, new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            PropertyNamingPolicy = jsonNamingPolicy,
        });
    }

    public static string[] SplitWithTrimming(this string input, string separator)
    {
        return input.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}