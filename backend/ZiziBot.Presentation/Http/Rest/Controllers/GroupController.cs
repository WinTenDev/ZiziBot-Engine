using Microsoft.AspNetCore.Mvc;
using RestDeleteWelcomeMessageRequest = ZiziBot.Application.Features.Handlers.RestApis.Group.DeleteWelcomeMessageRequest;
using RestGetWelcomeMessageRequest = ZiziBot.Application.Features.Handlers.RestApis.Group.GetWelcomeMessageRequest;
using RestListWelcomeMessageRequest = ZiziBot.Application.Features.Handlers.RestApis.Group.ListWelcomeMessageRequest;
using RestSaveWelcomeMessageRequest = ZiziBot.Application.Features.Handlers.RestApis.Group.SaveWelcomeMessageRequest;
using RestSelectWelcomeMessageRequest = ZiziBot.Application.Features.Handlers.RestApis.Group.SelectWelcomeMessageRequest;
using ZiziBot.Presentation.Security.Rbac;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ApiControllerBase
{
    [HttpGet("welcome-message")]
    [AccessFilter(flag: Flag.REST_GROUP_WELCOME_MESSAGE_GET_LIST, roleLevel: RoleLevel.ChatAdmin)]
    public async Task<IActionResult> ListWelcomeMessage(RestListWelcomeMessageRequest request)
    {
        return await SendRequest(request);
    }

    [HttpGet("welcome-message/{WelcomeId}")]
    [AccessFilter(flag: Flag.REST_GROUP_WELCOME_MESSAGE_GET_DETAIL, roleLevel: RoleLevel.ChatAdmin)]
    public async Task<IActionResult> GetWelcomeMessage(RestGetWelcomeMessageRequest request)
    {
        return await SendRequest(request);
    }

    [HttpPost("welcome-message")]
    [AccessFilter(flag: Flag.REST_GROUP_WELCOME_MESSAGE_CREATE, roleLevel: RoleLevel.ChatAdmin)]
    public async Task<IActionResult> SaveWelcomeMessage(RestSaveWelcomeMessageRequest request)
    {
        return await SendRequest(request);
    }

    [HttpDelete("welcome-message")]
    [AccessFilter(flag: Flag.REST_GROUP_WELCOME_MESSAGE_DELETE, roleLevel: RoleLevel.ChatAdmin)]
    public async Task<IActionResult> DeleteWelcomeMessage(RestDeleteWelcomeMessageRequest request)
    {
        return await SendRequest(request);
    }

    [HttpPost("select-welcome-message")]
    [AccessFilter(flag: Flag.REST_GROUP_WELCOME_MESSAGE_SELECT, roleLevel: RoleLevel.ChatAdmin)]
    public async Task<IActionResult> SelectWelcomeMessage(RestSelectWelcomeMessageRequest request)
    {
        return await SendRequest(request);
    }
}
