using Microsoft.AspNetCore.Mvc;
using RestDeleteNoteRequest = ZiziBot.Application.Features.Handlers.RestApis.Note.DeleteNoteRequest;
using RestGetNoteRequest = ZiziBot.Application.Features.Handlers.RestApis.Note.GetNoteRequest;
using RestListNoteRequest = ZiziBot.Application.Features.Handlers.RestApis.Note.ListNoteRequest;
using RestSaveNoteRequest = ZiziBot.Application.Features.Handlers.RestApis.Note.SaveNoteRequest;
using ZiziBot.Presentation.Security.Rbac;

namespace ZiziBot.Presentation.Http.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoteController : ApiControllerBase
{
    [HttpGet()]
    [AccessFilter(flag: Flag.REST_CHAT_NOTE_LIST, roleLevel: RoleLevel.ChatAdmin)]
    public async Task<IActionResult> GetListNotes([FromQuery] RestListNoteRequest request)
    {
        return await SendRequest(request);
    }

    [HttpGet("{NoteId}")]
    [AccessFilter(flag: Flag.REST_CHAT_NOTE_GET, roleLevel: RoleLevel.ChatAdmin)]
    public async Task<IActionResult> GetDetailNote(RestGetNoteRequest request)
    {
        return await SendRequest(request);
    }

    [HttpPost()]
    [AccessFilter(flag: Flag.REST_CHAT_NOTE_CREATE, roleLevel: RoleLevel.ChatAdmin)]
    public async Task<IActionResult> CreateNote(RestSaveNoteRequest request)
    {
        return await SendRequest(request);
    }

    [HttpDelete]
    [AccessFilter(flag: Flag.REST_CHAT_NOTE_DELETE, roleLevel: RoleLevel.ChatAdmin)]
    public async Task<IActionResult> DeleteNote(RestDeleteNoteRequest request)
    {
        return await SendRequest(request);
    }
}
