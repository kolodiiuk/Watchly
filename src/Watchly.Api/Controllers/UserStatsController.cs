using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Application.Interfaces;
using Watchly.Application.Models;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UserStatsController : BaseController<UserStatsController>
{
    private readonly IUserStatsService _userStatsService;

    public UserStatsController(IUserStatsService userStatsService,
        ILogger<UserStatsController> logger) : base(logger)
    {
        _userStatsService = userStatsService;
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
