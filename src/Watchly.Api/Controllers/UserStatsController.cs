using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UserStatsController : BaseController<UserStatsController>
{
    public UserStatsController(ILogger<UserStatsController> logger) : base(logger)
    {
    }

    [Authorize]
    [EndpointSummary("Gets movie stats.")]
    [EndpointDescription("Retrieves movie watching statistics for the user.")]
    [ProducesResponseType(typeof(MovieStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("movie")]
    public async Task<ActionResult<MovieStatsResponse>> GetMovieStatsAsync(CancellationToken ct)
    {
        return StatusCode(418);
    }

    [Authorize]
    [EndpointSummary("Gets series stats.")]
    [EndpointDescription("Retrieves series watching statistics for the user.")]
    [ProducesResponseType(typeof(SeriesStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpGet("series")]
    public async Task<ActionResult<SeriesStatsResponse>> GetSeriesStatsAsync(CancellationToken ct)
    {
        return StatusCode(418);
    }
}

public class SeriesStatsResponse
{
}

public class MovieStatsResponse
{
}
