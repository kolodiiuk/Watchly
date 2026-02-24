using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EpisodeQueryingController : BaseController<EpisodeQueryingController>
{
    public EpisodeQueryingController(ILogger<EpisodeQueryingController> logger) : base(logger)
    {
    }

    [EndpointSummary("Gets episode info.")]
    [EndpointDescription("Retrieves detailed information about a specific episode by its ID.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{episodeId:int}")]
    public async Task<IActionResult> GetEpisodeInfoAsync(int episodeId, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Gets season episodes.")]
    [EndpointDescription("Retrieves information about all episodes in a specified season.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("season/{seasonId:int}")]
    public async Task<IActionResult> GetEpisodeInfoPerSeasonAsync(int seasonId, CancellationToken ct)
    {
        return StatusCode(418);
    }
}
