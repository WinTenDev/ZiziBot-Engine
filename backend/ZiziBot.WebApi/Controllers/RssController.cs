using Microsoft.AspNetCore.Mvc;

namespace ZiziBot.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RssController : ApiControllerBase
{
    [HttpGet()]
    public async Task<IActionResult> Index(GetListRssRequest request)
    {
        return await SendRequest(request);
    }
}