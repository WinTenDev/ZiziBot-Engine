using Microsoft.AspNetCore.Mvc;
using ZiziBot.Presentation.Security.Rbac;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api/mirror-user")]
public class MirrorUserController : ApiControllerBase
{
    [HttpGet()]
    [AccessFilter(flag: Flag.REST_MIRROR_USER_GET_LIST, roleLevel: RoleLevel.Sudo)]
    public async Task<IActionResult> GetUsersAll(GetMirrorUsersRequestDto request)
    {
        return await SendRequest(request);
    }

    [HttpGet("check-user")]
    [AccessFilter(flag: Flag.REST_MIRROR_USER_CHECK_USER, roleLevel: RoleLevel.None)]
    public async Task<IActionResult> GetUserByUserId([FromQuery] GetMirrorUserByUserIdRequestDto request)
    {
        return await SendRequest(request);
    }

    [HttpPost()]
    [AccessFilter(flag: Flag.REST_MIRROR_USER_CREATE, roleLevel: RoleLevel.Sudo)]
    public async Task<IActionResult> PostUserMirror([FromBody] PostMirrorUserRequestDto request)
    {
        return await SendRequest(request);
    }

    [HttpDelete]
    [AccessFilter(flag: Flag.REST_MIRROR_USER_DELETE, roleLevel: RoleLevel.Sudo)]
    public async Task<IActionResult> DeleteMirrorUser([FromQuery] DeleteMirrorUserRequestDto request)
    {
        return await SendRequest(request);
    }

    [HttpPost("submit-donation")]
    [AccessFilter(flag: Flag.REST_MIRROR_SUBMIT_DONATION, roleLevel: RoleLevel.None)]
    public async Task<IActionResult> SubmitDonation(SubmitDonationRequest request)
    {
        return await SendRequest(request);
    }

    [HttpGet("check-order")]
    [AccessFilter(flag: Flag.REST_MIRROR_USER_CHECK_ORDER, roleLevel: RoleLevel.None)]
    public async Task<IActionResult> Index([FromQuery] string orderId)
    {
        return await SendRequest(new CheckPaymentOrderIdRequest() {
            OrderId = orderId
        });
    }

    [HttpPost("verify-user")]
    [AccessFilter(flag: Flag.REST_MIRROR_VERIFY_USER, roleLevel: RoleLevel.None)]
    public async Task<IActionResult> VerifyUser(VerifyUserRequest request)
    {
        return await SendRequest(request);
    }

    [HttpPost("trakteer-webhook")]
    [AccessFilter(flag: Flag.REST_MIRROR_TRAKTEER_WEBHOOK, roleLevel: RoleLevel.None)]
    public async Task<IActionResult> WebHookTrakteer(WebHookTrakteerDonationRequest request)
    {
        return await SendRequest(request);
    }
}
