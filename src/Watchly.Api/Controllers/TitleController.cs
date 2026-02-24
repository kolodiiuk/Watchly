using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Vote;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TitleController : BaseController<TitleController>
{
    public TitleController(ILogger<TitleController> logger) : base(logger)
    {
    }

    [EndpointSummary("Marks an episode as watched.")]
    [EndpointDescription("Records that the user has watched the specified episode in this title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    [HttpPost("watch/{episodeId:int}")]
    public async Task<IActionResult> MarkWatchedAsync(int episodeId, CancellationToken ct)
    {
        return StatusCode(418);
    }

    //todo: pagination
    [EndpointSummary("Gets comments for a title.")]
    [EndpointDescription("Retrieves all comments associated with the specified title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{titleId:int}/comments")]
    public async Task<IActionResult> GetCommentsAsync(int titleId, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Votes on a title.")]
    [EndpointDescription("Submits a user's vote or rating for a specific title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPost]
    public async Task<IActionResult> VoteAsync(int titleId, VoteDto voteDto, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Changes a vote on a title.")]
    [EndpointDescription("Updates a user's existing vote or rating for a specific title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> ChangeVoteAsync(int titleId, ChangeVoteDto changeVoteDto,
        CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Leaves a comment on a title.")]
    [EndpointDescription("Adds a new user comment to the specified title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpPost("comment")]
    public async Task<IActionResult> LeaveCommentAsync()
    {
        return StatusCode(418);
    }
}
