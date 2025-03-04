using Microsoft.AspNetCore.Mvc;
using Simplebank.API.Requests.Entries;
using Simplebank.Domain.Interfaces.Services;

namespace Simplebank.API.Controllers;

[ApiController]
[Route("[controller]")]
public class EntriesController : ControllerBase
{
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<ActionResult> GetEntryByIdAsync([FromRoute] Guid id, IEntriesService entriesService, [FromQuery] GetEntriesRequest request)
    {
        var entry = await entriesService.GetEntriesAsync(id, request.Page, request.PerPage);
        return Ok(entry);
    }
}