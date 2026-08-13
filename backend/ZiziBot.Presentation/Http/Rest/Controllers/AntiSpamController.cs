using Microsoft.AspNetCore.Mvc;
using ZiziBot.Presentation.Security.Rbac;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AntiSpamController : ApiControllerBase
{
    [HttpPost("ess-ban")]
    [AccessFilter(flag: Flag.REST_ANTISPAM_ESS_CREATE, roleLevel: RoleLevel.Sudo)]
    public async Task<IActionResult> PostEs2BanListAsync(PostGlobalBanApiRequest request)
    {
        return await SendRequest(request);
    }

    [HttpDelete("ess-ban")]
    [AccessFilter(flag: Flag.REST_ANTISPAM_ESS_DELETE, roleLevel: RoleLevel.Sudo)]
    public async Task<IActionResult> DeleteEs2BanListAsync([FromBody] DeleteGlobalBanApiRequest request)
    {
        return await SendRequest(request);
    }

    [HttpGet("ess-ban")]
    [AccessFilter(flag: Flag.REST_ANTISPAM_ESS_GET_LIST, roleLevel: RoleLevel.Sudo)]
    public async Task<IActionResult> GetEs2BanListAsync(GetGlobalBanApiRequest request)
    {
        return await SendRequest(request);
    }

    [HttpGet("check-ban")]
    [AccessFilter(flag: Flag.REST_ANTISPAM_ESS_CHECK_BAN, roleLevel: RoleLevel.None)]
    public async Task<IActionResult> GetEs2BanByUserIdAsync()
    {
        return await SendRequest(new GetGlobalBanApiRequest());
    }

    [HttpPut("undelete-ban")]
    [AccessFilter(flag: Flag.REST_ANTISPAM_ESS_UNDELETE, roleLevel: RoleLevel.Sudo)]
    public async Task<IActionResult> UndeleteEs2BanByUserIdAsync([FromBody] UndeleteGlobalBanApiRequest request)
    {
        return await SendRequest(request);
    }
}
