using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using ZiziBot.Application.Common.Dtos;
using ZiziBot.Application.Core.Interfaces;

namespace ZiziBot.Presentation.Infrastructure.Http;

public class HttpContextHelper(IHttpContextAccessor httpContextAccessor) : IHttpContextHelper
{
    internal HttpContext Context => httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available");

    public string RequestPath => Context.Request.Path.Value ?? "";
    public RequestHeaders Headers => Context.Request.GetTypedHeaders();
    public IHeaderDictionary HeaderDict => Headers.Headers;
    internal string UserAgent => Headers.Headers.UserAgent.ToString();

    public UserInfo UserInfo => Context.Items.TryGetValue(ValueConst.USER_INFO, out var userInfo) ? (UserInfo)userInfo : new UserInfo();
    public string TransactionId => Context.GetTransactionId();

    public async Task<string> GetRawBodyAsync()
    {
        Context.Request.EnableBuffering(); // Allow multiple reads
        Context.Request.Body.Position = 0;

        using var reader = new StreamReader(Context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        Context.Request.Body.Position = 0; // Reset for other consumers

        return body;
    }

    public async Task<T?> GetBodyAsJsonAsync<T>() where T : class
    {
        var json = await GetRawBodyAsync();
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<Dictionary<string, string>> GetFormDataAsync()
    {
        if (!Context.Request.HasFormContentType)
            return new Dictionary<string, string>();

        var form = await Context.Request.ReadFormAsync();
        return form.ToDictionary(f => f.Key, f => f.Value.ToString());
    }
}

