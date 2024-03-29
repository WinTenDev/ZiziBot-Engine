using System.Net;
using Newtonsoft.Json;

namespace ZiziBot.Application.Core;

public class ApiResponseBase<TResult>
{
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? TransactionId { get; set; }

    public string Message { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TResult? Result { get; set; }

    public ApiResponseBase<TResult> Success(string message, TResult? result = default)
    {
        StatusCode = HttpStatusCode.OK;
        Message = message;
        Result = result;

        return this;
    }

    public ApiResponseBase<TResult> Unauthorized(string message, TResult? result = default)
    {
        StatusCode = HttpStatusCode.Unauthorized;
        Message = message;
        Result = result;

        return this;
    }

    public ApiResponseBase<TResult> BadRequest(string message, TResult? result = default)
    {
        StatusCode = HttpStatusCode.BadRequest;
        Message = message;
        Result = result;

        return this;
    }

    public ApiResponseBase<TResult> NotFound(string message, TResult? result = default)
    {
        StatusCode = HttpStatusCode.NotFound;
        Message = message;
        Result = result;

        return this;
    }
}