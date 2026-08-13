using Microsoft.AspNetCore.Mvc;
using ZiziBot.Application.Features.UseCases.AppSetting;
using ZiziBot.Presentation.Security.Rbac;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppSettingController(GetListAppSettingUseCase getListAppSettingUseCase) : ApiControllerBase
{
    [HttpGet]
    [AccessFilter(flag: Flag.REST_APP_SETTING_GET_LIST, roleLevel: RoleLevel.Sudo)]
    public async Task<IActionResult> GetList(GetListAppSettingRequest request)
    {
        return await SendRequest(() => getListAppSettingUseCase.Handle(request));
    }
}
