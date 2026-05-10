using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Application.Interfaces;
using Watchly.Application.Services;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class WatchListController : BaseController<WatchListController>
{
    private readonly IWatchListService _watchListService;
    public WatchListController(ILogger<WatchListController> logger, IWatchListService watchListService) : base(logger)
    {
        _watchListService = watchListService;
    }

    [EndpointSummary("Adds title to to-watch list.")]
    [EndpointDescription("Adds the specified title to the user's to-watch list.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("to-watch")]
    public async Task<IActionResult> AddTitleToWatchListAsync(int titleId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.AddTitleToWatchListAsync(titleId, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Adding to watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

    [EndpointSummary("Removes title from to-watch list.")]
    [EndpointDescription("Removes the specified title from the user's to-watch list.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpDelete("to-watch/{titleId:int}")]
    public async Task<IActionResult> RemoveTitleFromToWatchAsync(int titleId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.RemoveTitleFromWatchListAsync(titleId, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Adding to watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

    [EndpointSummary("Creates a new custom watchlist.")]
    [EndpointDescription("Creates a new custom watchlist for the user with the given name.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("new")]
    public async Task<IActionResult> CreateCustWatchListAsync(string name, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.CreateCustWatchListAsync(name, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Creating custom watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok();
    }

    [EndpointSummary("Adds title to custom watchlist.")]
    [EndpointDescription("Adds the specified title to a specific custom watchlist.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("add")]
    public async Task<IActionResult> AddTitleToCustWatchListAsync(
        int titleId, int watchListId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.AddTitleToCustWatchListAsync(titleId, watchListId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Adding to watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

    [EndpointSummary("Removes title from custom watchlist.")]
    [EndpointDescription("Removes the specified title from a custom watchlist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpDelete("cust/{titleId:int}/{watchListId:int}")]
    public async Task<IActionResult> RemoveTitleFromCustWatchListAsync(int titleId, int watchListId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }
        var res = await _watchListService.RemoveTitleFromCustWatchListAsync(titleId, watchListId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Removing from watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok();
    }
}
