using Microsoft.AspNetCore.Mvc;
using ZiziBot.Presentation.Security.Rbac;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ApiControllerBase
{
    [HttpPost("{targetId}")]
    [AccessFilter(flag: Flag.REST_PRODUCTIVITY_WEBHOOK, roleLevel: RoleLevel.None)]
    public async Task<IActionResult> ProcessingPayload(PostWebhookPayloadRequest request)
    {
        return await SendRequest(request);
    }
}
