using Microsoft.AspNetCore.Mvc;
using ZiziBot.Application.Features.Handlers.RestApis.Pendekin;
using ZiziBot.Presentation.Security.Rbac;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PendekinController : ApiControllerBase
{
    [HttpPost]
    [AccessFilter(flag: Flag.REST_PRODUCTIVITY_PENDEKIN_CREATE, roleLevel: RoleLevel.User)]
    public async Task<IActionResult> CreatePendekin(CreatePendekinRequest request)
    {
        return await SendRequest(request);
    }

    [HttpGet("{ShortPath}")]
    [AccessFilter(flag: Flag.REST_PRODUCTIVITY_PENDEKIN_GET, roleLevel: RoleLevel.None)]
    public async Task<IActionResult> GetPendekin(GetPendekinRequest request)
    {
        return await SendRequest(request);
    }

    [HttpGet()]
    [AccessFilter(flag: Flag.REST_PRODUCTIVITY_PENDEKIN_LIST, roleLevel: RoleLevel.User)]
    public async Task<IActionResult> ListPendekin(ListPendekinRequest request)
    {
        return await SendRequest(request);
    }

    [HttpDelete("{ShortPath}")]
    [AccessFilter(flag: Flag.REST_PRODUCTIVITY_PENDEKIN_DELETE, roleLevel: RoleLevel.User)]
    public async Task<IActionResult> DeletePendekin(DeletePendekinRequest request)
    {
        return await SendRequest(request);
    }
}
