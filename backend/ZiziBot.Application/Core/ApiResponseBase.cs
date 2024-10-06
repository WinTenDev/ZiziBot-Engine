using System.Net;
using System.Text.Json.Serialization;

namespace ZiziBot.Application.Core;

public class ApiResponseBase<TResult>
{
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? TransactionId { get; set; }

    public string Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
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

public static class ApiResponseBase
{
    public static ApiResponseBase<TResult> ReturnSuccess<TResult>(string message, TResult? result = default)
    {
        return new ApiResponseBase<TResult>().Success(message, result);
    }

    public static ApiResponseBase<TResult> ReturnBadRequest<TResult>(string message, TResult? result = default)
    {
        return new ApiResponseBase<TResult>().BadRequest(message, result);
    }
}