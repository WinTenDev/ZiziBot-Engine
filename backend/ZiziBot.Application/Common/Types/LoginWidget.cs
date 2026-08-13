using ZiziBot.Application.Common.Enums;
using ZiziBot.Application.Common.Utils;

namespace ZiziBot.Application.Common.Types;

// Original source: https://github.com/TelegramBots/Telegram.Bot.Extensions.LoginWidget/blob/master/src/Telegram.Bot.Extensions.LoginWidget/LoginWidget.cs

/// <summary>
/// A helper class used to verify authorization data
/// </summary>
public class LoginWidget : IDisposable
{
    private readonly string _token;
    /// <summary>
    /// How old (in seconds) can authorization attempts be to be considered valid (compared to the auth_date field)
    /// </summary>
    private readonly long _allowedTimeOffset = 300000;

    private bool _disposed = false;
    private static readonly DateTime UnixStart = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Construct a new <see cref="LoginWidget"/> instance
    /// </summary>
    /// <param name="token">The bot API token used as a secret parameter when checking authorization</param>
    public LoginWidget(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        _token = token;
    }

    /// <summary>
    /// Checks whether the authorization data received from the user is valid
    /// </summary>
    /// <param name="fields">A collection containing query string fields as sorted key-value pairs</param>
    /// <returns></returns>
    public WebAuthorization CheckAuthorization(SortedDictionary<string, string> fields)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(LoginWidget));
        if (fields == null) throw new ArgumentNullException(nameof(fields));

        if (fields.Count < 3) return WebAuthorization.MissingFields;

        if (!fields.ContainsKey(Field.Id) ||
            !fields.TryGetValue(Field.AuthDate, out string authDate) ||
            !fields.TryGetValue(Field.Hash, out string hash)
           ) return WebAuthorization.MissingFields;

        if (hash.Length != 64)
            return WebAuthorization.InvalidHash;

        if (!long.TryParse(authDate, out long timestamp))
            return WebAuthorization.InvalidAuthDateFormat;

        if (Math.Abs(DateTime.UtcNow.Subtract(UnixStart).TotalSeconds - timestamp) > _allowedTimeOffset)
            return WebAuthorization.TooOld;

        fields.Remove(Field.Hash);

        var dataCheckString = string.Join("\n", fields.Select(f => $"{f.Key}={f.Value}"));

        var myHash = StringUtil.HashHmac(_token, dataCheckString);

        return hash != myHash ? WebAuthorization.InvalidHash : WebAuthorization.Valid;

    }

    /// <summary>
    /// Checks whether the authorization data received from the user is valid
    /// </summary>
    /// <param name="fields">A collection containing query string fields as key-value pairs</param>
    /// <returns></returns>
    public WebAuthorization CheckAuthorization(Dictionary<string, string> fields)
    {
        if (fields == null) throw new ArgumentNullException(nameof(fields));

        return CheckAuthorization(new SortedDictionary<string, string>(fields, StringComparer.Ordinal));
    }

    /// <summary>
    /// Checks whether the authorization data received from the user is valid
    /// </summary>
    /// <param name="fields">A collection containing query string fields as key-value pairs</param>
    /// <returns></returns>
    public WebAuthorization CheckAuthorization(IEnumerable<KeyValuePair<string, string>> fields) =>
        CheckAuthorization(fields?.ToDictionary(f => f.Key, f => f.Value, StringComparer.Ordinal));

    /// <summary>
    /// Checks whether the authorization data received from the user is valid
    /// </summary>
    /// <param name="fields">A collection containing query string fields as key-value pairs</param>
    /// <returns></returns>
    public WebAuthorization CheckAuthorization(IEnumerable<Tuple<string, string>> fields) =>
        CheckAuthorization(fields?.ToDictionary(f => f.Item1, f => f.Item2, StringComparer.Ordinal));

    public void Dispose()
    {
        if (!_disposed)
        {
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }

    private static class Field
    {
        public const string AuthDate = "auth_date";
        public const string Id = "id";
        public const string Hash = "hash";
    }
}