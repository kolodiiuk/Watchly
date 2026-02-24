using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class WatchListController : BaseController<WatchListController>
{
    public WatchListController(ILogger<WatchListController> logger) : base(logger)
    {
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
        return StatusCode(418);
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
        return StatusCode(418);
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
        return StatusCode(418);
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
        return StatusCode(418);
    }

    [EndpointSummary("Removes title from custom watchlist.")]
    [EndpointDescription("Removes the specified title from a custom watchlist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpDelete("cust/{titleId:int}")]
    public async Task<IActionResult> RemoveTitleFromCustWatchListAsync(int titleId, CancellationToken ct)
    {
        return StatusCode(418);
    }
}
