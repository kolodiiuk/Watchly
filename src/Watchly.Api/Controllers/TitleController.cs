using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Vote;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TitleController : BaseController<TitleController>
{
    public TitleController(ILogger<TitleController> logger) : base(logger)
    {
    }

    [EndpointSummary("Marks an episode as watched.")]
    [EndpointDescription("Records that the user has watched the specified episode in this title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    [HttpPost("watch/{episodeId:int}")]
    public async Task<IActionResult> MarkWatchedAsync(int episodeId, CancellationToken ct)
    {
        return StatusCode(418);
    }
}
