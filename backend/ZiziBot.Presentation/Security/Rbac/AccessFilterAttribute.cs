using Microsoft.AspNetCore.Mvc;

namespace ZiziBot.Presentation.Security.Rbac;

[AttributeUsage(AttributeTargets.Method)]
public class AccessFilterAttribute : TypeFilterAttribute
{
    public AccessFilterAttribute(
        string flag,
        RoleLevel roleLevel
    ) : base(typeof(AccessFilterAuthorizationFilter))
    {
        Arguments = [
            flag,
            roleLevel
        ];
    }
}

