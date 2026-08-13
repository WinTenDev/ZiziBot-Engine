using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

public class ApiControllerBase : ControllerBase
{
    private MediatorService MediatorService => HttpContext.RequestServices.GetRequiredService<MediatorService>();

    protected async Task<IActionResult> SendRequest<T>(ApiRequestBase<T> request, ExecutionStrategy executionStrategy = ExecutionStrategy.Instant)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await MediatorService.EnqueueAsync(request, executionStrategy);
        result.ExecutionTime = stopwatch.Elapsed;
        stopwatch.Stop();

        return SwitchStatus(result);
    }

    protected async Task<IActionResult> SendRequest<T>(Func<Task<ApiResponseBase<T>>> request)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await request();
        response.ExecutionTime = stopwatch.Elapsed;
        stopwatch.Stop();

        return SwitchStatus(response);
    }

    protected IActionResult SwitchStatus<T>(ApiResponseBase<T> responseBase)
    {
        responseBase.TransactionId = HttpContext.GetTransactionId();

        return responseBase.StatusCode switch {
            HttpStatusCode.OK => Ok(responseBase),
            HttpStatusCode.BadRequest => BadRequest(responseBase),
            HttpStatusCode.Unauthorized => Unauthorized(responseBase),
            HttpStatusCode.NotFound => NotFound(responseBase),
            _ => BadRequest(responseBase)
        };
    }
}
