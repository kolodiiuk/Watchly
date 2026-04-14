using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Filters;
using Watchly.Application.Interfaces;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class VoteController : BaseController<VoteController>
{
    private readonly IVoteService _voteService;

    public VoteController(IVoteService voteService, ILogger<VoteController> logger) : base(logger)
    {
        _voteService = voteService;
    }

    [EndpointSummary("Votes on a title.")]
    [EndpointDescription("Submits a user's vote or rating for a specific title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("title/{titleId:int}/{value:int}")]
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> VoteTitleAsync(int titleId, short value, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _voteService.VoteTitleAsync(titleId, value, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Vote failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

    [EndpointSummary("Changes a vote on a title.")]
    [EndpointDescription("Updates a user's existing vote or rating for a specific title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPatch("title/{voteId:int}/{value:int}")]
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> ChangeVoteTitleAsync(int voteId, short value, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _voteService.ChangeVoteAsync(voteId, value, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Change vote failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

    [EndpointSummary("Votes on an episode.")]
    [EndpointDescription("Submits a user's vote or rating for a specific episode.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("episode/{episodeId:int}/{value:int}")]
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> VoteAsync(int episodeId, short value, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _voteService.VoteEpisodeAsync(episodeId, value, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Vote failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

    [EndpointSummary("Changes a vote on an episode.")]
    [EndpointDescription("Updates a user's existing vote or rating for a specific episode.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPatch("episode/{voteId:int}/{value:int}")]
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> ChangeVoteAsync(int voteId, short value, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _voteService.ChangeVoteAsync(voteId, value, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Change vote failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }
}
