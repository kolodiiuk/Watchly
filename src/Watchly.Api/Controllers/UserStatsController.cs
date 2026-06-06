using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Stats;

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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("movie")]
    public async Task<ActionResult<MovieStatsResponse>> GetMovieStatsAsync(CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        ct.ThrowIfCancellationRequested();
        var res = await _userStatsService.GetUserMovieStatsAsync(UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Get movie stats failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }

    [Authorize]
    [EndpointSummary("Gets series stats.")]
    [EndpointDescription("Retrieves series watching statistics for the user.")]
    [ProducesResponseType(typeof(SeriesStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("series")]
    public async Task<ActionResult<SeriesStatsResponse>> GetSeriesStatsAsync(CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }
        
        ct.ThrowIfCancellationRequested();
        var res = await _userStatsService.GetUserTvSeriesStatsAsync(UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Get series stats failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }
}
