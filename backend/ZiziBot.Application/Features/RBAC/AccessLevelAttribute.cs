using Microsoft.AspNetCore.Mvc;

namespace ZiziBot.Application.Features.RBAC;

[AttributeUsage(AttributeTargets.Method)]
public class AccessLevelAttribute : TypeFilterAttribute
{

	public AccessLevelAttribute(AccessLevel accessLevel) : base(typeof(AccessFilter))
	{
		Arguments = new object[] { accessLevel };
	}
}