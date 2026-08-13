using Microsoft.AspNetCore.Mvc;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api")]
public class RootController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(GetWelcomeRequest request)
    {
        return await SendRequest(request);
    }
}

