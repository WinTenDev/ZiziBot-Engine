using Microsoft.AspNetCore.Mvc;

namespace ZiziBot.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PendekinController : ApiControllerBase
{
    [HttpPost]
    [AccessFilter]
    public async Task<IActionResult> CreatePendekin(CreatePendekinRequest request)
    {
        return await SendRequest(request);
    }

    [HttpGet("{ShortPath}")]
    [AccessFilter]
    public async Task<IActionResult> GetPendekin(GetPendekinRequest request)
    {
        return await SendRequest(request);
    }

    [HttpGet()]
    [AccessFilter]
    public async Task<IActionResult> ListPendekin(ListPendekinRequest request)
    {
        return await SendRequest(request);
    }
}