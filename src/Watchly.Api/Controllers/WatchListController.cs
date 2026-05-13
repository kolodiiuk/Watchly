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

    [EndpointSummary("Adds title to default to-watch list.")]
    [EndpointDescription("Adds the specified title to the user's default to-watch list.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("to-watch/")]
    public async Task<IActionResult> AddTitleToDefaultWatchListAsync(int titleId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.AddTitleToDefaultWatchListAsync(titleId, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Adding to default watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

    [EndpointSummary("Removes title from default to-watch list.")]
    [EndpointDescription("Removes the specified title from the user's default to-watch list.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpDelete("to-watch/")]
    public async Task<IActionResult> RemoveTitleFromDefaultWatchListAsync(int titleId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.RemoveTitleFromDefaultWatchListAsync(titleId, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Removing from default watch list failed",
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
    [HttpPost("to-watch/new")]
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

    [EndpointSummary("Adds title to specific watchlist.")]
    [EndpointDescription("Adds the specified title to a specific watchlist by id.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("to-watch/{watchListId:int}")]
    public async Task<IActionResult> AddTitleToWatchListByIdAsync(
        int titleId, int watchListId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.AddTitleToWatchListByIdAsync(titleId, watchListId, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Adding to watch list failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

    [EndpointSummary("Removes title from specific watchlist.")]
    [EndpointDescription("Removes the specified title from a specific watchlist by id.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpDelete("to-watch/{watchListId:int}")]
    public async Task<IActionResult> RemoveTitleFromWatchListByIdAsync(int titleId, int watchListId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }
        var res = await _watchListService.RemoveTitleFromWatchListByIdAsync(titleId, watchListId, UserId, ct);
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
    [HttpDelete("to-watch/delete/{watchListId:int}")]
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
    [HttpPatch("to-watch/{watchListId:int}")]
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
    [HttpGet("to-watch/titles")]
    public async Task<ActionResult<IEnumerable<Title>>> GetTitlesInDefaultWatchListAsync(CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.GetTitlesInDefaultWatchListAsync(UserId, ct);
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
    [HttpGet("to-watch/titles/{watchListId:int}")]
    public async Task<ActionResult<IEnumerable<TitleShortInfo>>> GetTitlesInWatchListByIdAsync(int watchListId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _watchListService.GetTitlesInWatchListByIdAsync(watchListId, UserId, ct);
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
    [HttpGet("to-watch/lists")]
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
    [HttpGet("to-watch/lists/{titleId:int}")]
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
