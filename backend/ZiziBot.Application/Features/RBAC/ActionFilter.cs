using Microsoft.AspNetCore.Mvc.Filters;

namespace ZiziBot.Application.Features.RBAC;

public class ActionFilter : IAsyncActionFilter
{
	public ActionFilter()
	{

	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{

	}
}