using Microsoft.AspNetCore.Mvc;

namespace ZiziBot.Application.Core;

public class ApiPostRequestBase<TBody, T> : ApiRequestBase<T>
{
    [FromBody]
    public TBody? Body { get; set; }
}