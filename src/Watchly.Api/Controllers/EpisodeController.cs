using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Vote;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class EpisodeController : BaseController<EpisodeController>
{
    public EpisodeController(ILogger<EpisodeController> logger) : base(logger)
    {
    }

    [EndpointSummary("Marks an episode as watched.")]
    [EndpointDescription("Records that the user has watched the specified episode.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("watch/{episodeId:int}")]
    public async Task<IActionResult> MarkWatchedAsync(int episodeId, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Votes on an episode.")]
    [EndpointDescription("Submits a user's vote or rating for a specific episode.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("vote/{episodeId:int}")]
    public async Task<IActionResult> VoteAsync(int episodeId, VoteDto voteDto, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Changes a vote on an episode.")]
    [EndpointDescription("Updates a user's existing vote or rating for a specific episode.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPut("vote/{episodeId:int}")]
    public async Task<IActionResult> ChangeVoteAsync(int episodeId, ChangeVoteDto changeVoteDto,
        CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Leaves a comment on an episode.")]
    [EndpointDescription("Adds a new user comment to the specified episode.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("comment/{episodeId:int}")]
    public async Task<IActionResult> LeaveCommentAsync(int episodeId, string text, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Updates a comment.")]
    [EndpointDescription("Modifies an existing comment by its ID.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPut("comment")]
    public async Task<IActionResult> UpdateCommentAsync(int commentId, string text, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Deletes a comment.")]
    [EndpointDescription("Removes a user comment by its ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpDelete("comment/{commnentId:int}")]
    public async Task<IActionResult> DeleteCommentAsync(int commentId, CancellationToken ct)
    {
        return StatusCode(418);
    }
}
