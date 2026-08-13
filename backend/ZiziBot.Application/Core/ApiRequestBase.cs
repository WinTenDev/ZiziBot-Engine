using Microsoft.AspNetCore.Mvc;

namespace ZiziBot.Application.Core;

public class ApiRequestBase<T> : IAppCommand<ApiResponseBase<T>>, IRestApiRequest
{
}

public class ApiRequestBase<T, TBody> : ApiRequestBase<T>
{
    [FromBody]
    public required TBody Body { get; set; }
}
