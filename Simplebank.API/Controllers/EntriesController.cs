using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simplebank.API.Authorization;
using Simplebank.API.Requests.Entries;
using Simplebank.Domain.Interfaces.Services;

namespace Simplebank.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class EntriesController : ControllerBase
{
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<ActionResult> GetEntryByIdAsync([FromRoute] Guid id, IEntriesService entriesService, [FromQuery] GetEntriesRequest request)
    {
        var tokenInfo = UserInfoExtractor.ExtractFromRequest(User);
        var entry = await entriesService.GetEntriesAsync(tokenInfo.UserId, id, request.Page, request.PerPage);
        return Ok(entry);
    }
}