using Microsoft.AspNetCore.Mvc;
using ZiziBot.Application.Features.UseCases.Rss;
using ZiziBot.Presentation.Security.Rbac;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RssController(GetRssHistoryUseCase getRssHistoryUseCase) : ApiControllerBase
{
    [HttpGet()]
    [AccessFilter(flag: Flag.REST_CHAT_RSS_LIST, roleLevel: RoleLevel.User)]
    public async Task<IActionResult> Index(GetListRssRequest request)
    {
        return await SendRequest(request);
    }

    [HttpGet("history")]
    [AccessFilter(flag: Flag.REST_CHAT_RSS_LIST_HISTORY, roleLevel: RoleLevel.User)]
    public async Task<IActionResult> GetHistory(GetRssHistoryRequest request)
    {
        return await SendRequest(() => getRssHistoryUseCase.Handle(request));
    }

    [HttpPost]
    [AccessFilter(flag: Flag.REST_CHAT_RSS_CREATE, roleLevel: RoleLevel.User)]
    public async Task<IActionResult> Save(SaveRssRequest request)
    {
        return await SendRequest(request);
    }
}
