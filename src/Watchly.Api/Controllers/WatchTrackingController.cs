using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Application.Interfaces;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("api/track")]
public class WatchTrackingController : BaseController<WatchTrackingController>
{
    private readonly IWatchTrackingService _watchTrackingService;

    public WatchTrackingController(
        IWatchTrackingService watchTrackingService, 
        ILogger<WatchTrackingController> logger) : base(logger)
    {
        _watchTrackingService = watchTrackingService;
    }

    [HttpPost("movie/incr/{movieId:int}")]
    [Authorize]
    public async Task<IActionResult> IncrWatchCountMovieAsync(int movieId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _watchTrackingService.IncrWatchingCountMovieAsync(movieId, UserId, ct);
        if (res.Failure)
        {
            return Problem();
        }

        return NoContent();
    }

    [HttpPost("season/incr/{seasonId:int}")]
    [Authorize]
    public async Task<IActionResult> IncrWatchCountSeasonAsync(int seasonId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _watchTrackingService.IncrWatchingCountSeasonAsync(seasonId, UserId, ct);
        if (res.Failure)
        {
            return Problem();
        }

        return NoContent();
    }

    [HttpPost("episode/incr/{episodeId}")]
    [Authorize]
    public async Task<IActionResult> IncrWatchCountEpisodeAsync(int episodeId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _watchTrackingService.IncrWatchingCountEpisodeAsync(episodeId, UserId, ct);
        if (res.Failure)
        {
            return Problem();
        }

        return NoContent();
    }

    [HttpPost("movie/decr/{movieId:int}")]
    [Authorize]
    public async Task<IActionResult> DecrWatchCountMovieAsync(int movieId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _watchTrackingService.DecrWatchingCountMovieAsync(movieId, UserId, ct);
        if (res.Failure)
        {
            return Problem();
        }

        return NoContent();
    }

    [HttpPost("season/decr/{seasonId:int}")]
    [Authorize]
    public async Task<IActionResult> DecrWatchCountSeasonAsync(int seasonId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _watchTrackingService.DecrWatchingCountSeasonAsync(seasonId, UserId, ct);
        if (res.Failure)
        {
            return Problem();
        }

        return NoContent();
    }

    [HttpPost("episode/decr/{episodeId}")]
    [Authorize]
    public async Task<IActionResult> DecrWatchCountEpisodeAsync(int episodeId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _watchTrackingService.DecrWatchingCountEpisodeAsync(episodeId, UserId, ct);
        if (res.Failure)
        {
            return Problem();
        }

        return NoContent();
    }

    [HttpGet("movie/{movieId:int}")]
    [Authorize]
    public async Task<ActionResult<int>> GetMovieWatchCountAsync(int movieId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _watchTrackingService.GetWatchCountInfoMovieAsync(movieId, UserId, ct);
        if (res.Failure)
        {
            return Problem();
        }

        return Ok(res.Value);
    }

    [HttpGet("episode/{movieId:int}")]
    [Authorize]
    public async Task<ActionResult<int>> GetEpisodeWatchCountAsync(int episodeId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _watchTrackingService.GetWatchCountInfoEpisodeAsync(episodeId, UserId, ct);
        if (res.Failure)
        {
            return Problem();
        }

        return Ok(res.Value);
    }

    [HttpGet("tv-show/{tvShowId:int}")]
    [Authorize]
    public async Task<ActionResult<TvShowWatchInfo>> GetTvShowWatchCountAsync(int tvShowId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _watchTrackingService.GetWatchCountInfoTvShowAsync(tvShowId, UserId, ct);
        if (res.Failure)
        {
            return Problem();
        }

        return Ok(res.Value);
    }
}
