using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Ardalis.GuardClauses;
using Humanizer;
using ZiziBot.Application.Common.Enums;

namespace ZiziBot.Application.Common.Utils;

public static class ValueUtil
{
    public static T Convert<T>(this object? input, T defaultVal)
    {
        var converter = TypeDescriptor.GetConverter(typeof(T));

        if (converter != null)
        {
            return (T)converter.ConvertFromString(input.ToString());
        }

        return defaultVal;
    }

    public static T Convert<T>(this object? input)
    {
        return Convert(input, default(T));
    }


    public static TNumber Convert<TNumber>(this string? src)
    {
        if (src == null) return default;

        return Convert<TNumber>((object?)src?.Replace(".", ""));
    }

    public static bool TryConvert<T>(this object? input, T defaultVal, out T result)
    {
        try
        {
            result = Convert(input, defaultVal);
            return true;
        }
        catch (Exception exception)
        {
            result = defaultVal;
            return input == null;
        }
    }

    public static Dictionary<string, object> ToDictionary(this object? values, StringType stringType = StringType.Original)
    {
        var symbols = new Dictionary<string, object>();

        if (values != null)
        {
            var properties = values.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(values, null) ?? string.Empty;
                var propertyName = property.Name;
                propertyName = stringType switch
                {
                    StringType.Original => propertyName,
                    StringType.SnakeCase => propertyName.Underscore().ToLower(),
                    StringType.PascalCase => propertyName.Pascalize(),
                    StringType.TitleCase => propertyName.Titleize(),
                    _ => propertyName
                };

                symbols.Add(propertyName, value.ToString() ?? string.Empty);
            }
        }

        return symbols;
    }

    public static bool IsNullOrDefault<K, V>(this KeyValuePair<K, V>? pair)
    {
        if (pair == null) return true;

        var (key, value) = pair.Value;

        return EqualityComparer<K>.Default.Equals(key, default) ||
               EqualityComparer<V>.Default.Equals(value, default);
    }

    public static TDestination FromDictionary<TDestination>(this IDictionary<string, string> dictionary) where TDestination : new()
    {
        var obj = new TDestination();

        foreach (var propertyInfo in typeof(TDestination).GetProperties())
        {
            propertyInfo.SetValue(obj, dictionary[propertyInfo.Name]);
        }

        return obj;
    }

    public static string EnsureNotNullOrWhiteSpace([System.Diagnostics.CodeAnalysis.NotNull] [ValidatedNotNull] this string? obj)
    {
        return Guard.Against.NullOrWhiteSpace(obj);
    }

    public static T EnsureNotNull<T>([System.Diagnostics.CodeAnalysis.NotNull] [ValidatedNotNull] this T? obj) where T : class
    {
        return Guard.Against.Null(obj);
    }

    public static int GenerateOtp()
    {
        var otp = RandomNumberGenerator.GetInt32(100000, 999999);
        return otp;
    }
}