using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Logging;
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("movie")]
    public async Task<ActionResult<MovieStatsResponse>> GetMovieStatsAsync(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isParsed = Guid.TryParse(userId, out var guid);
        if (!isParsed)
        {
            Log(LogLevel.Warning, UserStatsControllerEventIds.GetMovieFailed,
                "User ID not found in claims or invalid when requesting movie stats");

            return Problem(
                title: "User ID validation failure",
                detail: "User ID not found in claims or invalid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, UserStatsControllerEventIds.GetMovieAttempt,
            "Get movie stats attempt for user {UserId}", userId);

        var res = await _userStatsService.GetUserMovieStatsAsync(guid, ct);
        if (res.Failure)
        {
            Log(LogLevel.Error, UserStatsControllerEventIds.GetMovieFailed,
                "Get movie stats for user {UserId} failed: {error}", userId, res.Error);

            return Problem(
                title: "Get movie stats failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }

    [Authorize]
    [EndpointSummary("Gets series stats.")]
    [EndpointDescription("Retrieves series watching statistics for the user.")]
    [ProducesResponseType(typeof(SeriesStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("series")]
    public async Task<ActionResult<SeriesStatsResponse>> GetSeriesStatsAsync(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isParsed = Guid.TryParse(userId, out var guid);
        if (!isParsed)
        {
            Log(LogLevel.Warning, UserStatsControllerEventIds.GetSeriesFailed,
                "User ID not found in claims or invalid when requesting series stats");

            return Problem(
                title: "User ID validation failure",
                detail: "User ID not found in claims or invalid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, UserStatsControllerEventIds.GetSeriesAttempt,
            "Get series stats attempt for user {UserId}", userId);

        var res = await _userStatsService.GetUserTvSeriesStatsAsync(guid, ct);
        if (res.Failure)
        {
            Log(LogLevel.Error, UserStatsControllerEventIds.GetSeriesFailed,
                "Get series stats for user {UserId} failed: {error}", userId, res.Error);

            return Problem(
                title: "Get series stats failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }
}
