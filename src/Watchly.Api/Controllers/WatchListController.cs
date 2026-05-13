using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Content;
using Watchly.Application.Models.WatchList;
using Watchly.Domain.Entities;

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

        var res = await _watchListService.AddTitleToCustWatchListAsync(titleId, watchListId, UserId, ct);
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
        var res = await _watchListService.RemoveTitleFromCustWatchListAsync(titleId, watchListId, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Removing from watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok();
    }

    [EndpointSummary("Deletes custom watchlist.")]
    [EndpointDescription("Deletes specified custom watchlist, default watchlists cannot be removed.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpDelete("cust/{watchListId:int}")]
    public async Task<IActionResult> DeleteCustWatchListAsync(int watchListId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.DeleteCustWatchListAsync(watchListId, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Deleting watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok();
    }

    [EndpointSummary("Renames custom watchlist.")]
    [EndpointDescription("Renames specified custom watchlist, default watchlists cannot be renamed.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPatch("cust/{watchListId:int}")]
    public async Task<IActionResult> RenameCustWatchListAsync(int watchListId, string newName, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }
        var res = await _watchListService.RenameCustWatchListAsync(watchListId, newName, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Renaming watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok();
    }

    [EndpointSummary("Gets titles in default watchlist.")]
    [EndpointDescription("Gets all titles in user's default watchlist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("default/titles")]
    public async Task<ActionResult<IEnumerable<Title>>> GetTitlesInWatchListAsync(CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.GetTitlesInWatchListAsync(UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Getting titles in default watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok(res.Value);
    }

    [EndpointSummary("Gets titles in watchlist.")]
    [EndpointDescription("Gets all titles in specified watchlist, user must be owner of the watchlist.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("{watchListId:int}/titles")]
    public async Task<ActionResult<IEnumerable<TitleShortInfo>>> GetTitlesInCustWatchListAsync(int watchListId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.GetTitlesInCustWatchListAsync(watchListId, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Getting titles in watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok(res.Value);
    }

    [EndpointSummary("Gets user's watchlists.")]
    [EndpointDescription("Gets short info of all watchlists owned by the user.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("user/watchlists")]
    public async Task<ActionResult<IEnumerable<WatchListInfo>>> GetUserWatchListsAsync(CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.GetUserWatchListsAsync(UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Getting user watch lists failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok(res.Value);
    }

    [EndpointSummary("Gets watchlists with a specific title.")]
    [EndpointDescription("Gets all watchlists that contain the specified title, user must be owner of the watchlists.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("title/{titleId:int}/watchlists")]
    public async Task<ActionResult<IEnumerable<WatchListShortInfo>>> GetWatchListsWithTitleAsync(int titleId, CancellationToken ct)
    {
        if(UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.GetWatchListsWithTitleAsync(titleId, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Getting watch lists with title failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok(res.Value);
    }
}
