using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ZiziBot.Application.Features.UseCases.User;
using ZiziBot.Presentation.Security.Rbac;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(CreateSessionOtpUseCase createSessionOtpUseCase) : ApiControllerBase
{
    [HttpGet("info")]
    [AccessFilter(flag: Flag.REST_USER_INFO_GET, roleLevel: RoleLevel.User)]
    public async Task<IActionResult> GetUserInfo(GetUserInfoRequest request)
    {
        return await SendRequest(request);
    }

    [HttpPost("session/telegram")]
    [AccessFilter(flag: Flag.REST_USER_TELEGRAM_SESSION_CREATE, roleLevel: RoleLevel.None)]
    public async Task<IActionResult> PostTelegramSession(ValidateTelegramSessionRequest request)
    {
        return await SendRequest(request);
    }

    [HttpPost("session/otp")]
    [AccessFilter(flag: Flag.REST_USER_SESSION_OTP_POST, roleLevel: RoleLevel.None)]
    public async Task<IActionResult> PostOtpSession([FromBody] CreateSessionOtpRequest request)
    {
        return await SendRequest(() => createSessionOtpUseCase.Handle(request));
    }

    [HttpGet("list-group")]
    [AccessFilter(flag: Flag.REST_USER_GROUP_LIST, roleLevel: RoleLevel.User)]
    [EnableRateLimiting(RateLimitingPolicy.API_LIST_RATE_LIMITING_KEY)]
    public async Task<IActionResult> GetListGroup([FromQuery] GetListGroupRequest request)
    {
        return await SendRequest(request);
    }
}
