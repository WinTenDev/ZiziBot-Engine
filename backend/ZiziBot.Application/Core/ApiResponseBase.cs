using System.Diagnostics;
using System.Net;
using System.Text.Json.Serialization;

namespace ZiziBot.Application.Core;

public class ApiResponseBase<TResult>
{
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? TransactionId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TimeSpan ExecutionTime { get; set; }

    public string Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorMessage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? ErrorMessages { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StackTrace? StackTrace { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiMetadata? Metadata { get; set; }

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

    public ApiResponseBase<TResult> SetMetadata(
        int pageSize = 0,
        int totalItem = 0,
        int pageNumber = 0,
        int actualPageSize = 0
    )
    {
        int totalPage;

        if (pageSize <= 0 || pageNumber <= 0)
        {
            pageNumber = 1;
            pageSize = 1;
            totalPage = 1;
        }
        else
        {
            totalPage = totalItem / pageSize + 1;
        }


        Metadata = new ApiMetadata
        {
            PageNumber = pageNumber,
            TotalItem = totalItem,
            TotalPage = totalPage,
            PageSize = actualPageSize,
            HasNext = pageNumber < totalPage
        };

        return this;
    }
}

public class ApiMetadata
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItem { get; set; }
    public int TotalPage { get; set; }
    public bool HasNext { get; set; }
}

public static class ApiResponse
{
    public static ApiResponseBase<T> Create<T>()
    {
        return new ApiResponseBase<T>();
    }
}