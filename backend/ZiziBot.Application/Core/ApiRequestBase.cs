using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ZiziBot.Application.Core;

public class ApiRequestBase<T> : IRequest<ApiResponseBase<T>>
{
    [FromServices]
    [SwaggerIgnore]
    public IHttpContextAccessor? HttpContextAccessor { get; set; }

    [BindNever]
    [SwaggerIgnore]
    public IHeaderDictionary Headers => HttpContextAccessor?.HttpContext?.Request.Headers!;

    [BindNever]
    [SwaggerIgnore]
    public string? TransactionId => HttpContextAccessor.GetTransactionId();

    [BindNever]
    [SwaggerIgnore]
    public string? Authorization => Headers[HeaderKey.Authorization];

    [BindNever]
    [SwaggerIgnore]
    public long SessionUserId => Headers.GetUserId();

    [BindNever]
    [SwaggerIgnore]
    public ApiRole? SessionUserRole => Headers[HeaderKey.UserRole].ToString().ToEnum(ApiRole.Guest);

    [BindNever]
    [SwaggerIgnore]
    public List<long> AdminChatId => Headers[HeaderKey.ListChatId].ToString().ToObject<List<long>>() ?? new List<long>();

    [BindNever]
    [SwaggerIgnore]
    public List<long> ListChatId => AdminChatId.Append(SessionUserId).Where(x => x != 0).ToList();

    [BindNever]
    [SwaggerIgnore]
    public string? BearerToken => Authorization?.Replace("Bearer ", string.Empty);

    public async Task<string> RequestBody() => await HttpContextAccessor?.HttpContext?.GetBodyAsync();
}