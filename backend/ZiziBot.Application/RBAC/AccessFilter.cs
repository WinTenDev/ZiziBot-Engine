using Microsoft.AspNetCore.Mvc.Filters;

namespace ZiziBot.Application.RBAC;

public class AccessFilter(AccessLevel accessLevel) : Attribute, IAsyncAuthorizationFilter
{
	private readonly AccessLevel _accessLevel = accessLevel;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		throw new NotImplementedException();
	}
}