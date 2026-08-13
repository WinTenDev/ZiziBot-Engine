using Microsoft.AspNetCore.Mvc;
using ZiziBot.Application.Features.UseCases.ApiKey;
using ZiziBot.Presentation.Security.Rbac;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiKeyController(GetListApiKeyUseCase getListApiKeyUseCase) : ApiControllerBase
{
    [HttpGet]
    [AccessFilter(flag: Flag.REST_API_KEY_GET, roleLevel: RoleLevel.Sudo)]
    public async Task<IActionResult> Index(GetListApiKeyRequest request)
    {
        return await SendRequest(() => getListApiKeyUseCase.Handle(request));
    }
}
