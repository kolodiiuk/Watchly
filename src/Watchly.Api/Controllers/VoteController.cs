using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Filters;
using Watchly.Application.Interfaces;
using ChangeVoteDto = Watchly.Application.Interfaces.ChangeVoteDto;
using VoteDto = Watchly.Application.Interfaces.VoteDto;

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
    [HttpPost("title")]
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> VoteTitleAsync(int titleId, VoteDto voteDto, CancellationToken ct)
    {
        var res = await _voteService.VoteTitleAsync(titleId, voteDto, UserId, ct);
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
    [HttpPut("title")]
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> ChangeVoteTitleAsync(int titleId, ChangeVoteDto changeVoteDto,
        CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _voteService.ChangeVoteTitleAsync(titleId, changeVoteDto, UserId, ct);
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
    [HttpPost("vote/{episodeId:int}")]
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> VoteAsync(int episodeId, VoteDto voteDto, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _voteService.VoteEpisodeAsync(episodeId, voteDto, UserId, ct);
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
    [HttpPut("{episodeId:int}")]
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> ChangeVoteAsync(int episodeId, ChangeVoteDto changeVoteDto,
        CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _voteService.ChangeVoteEpisodeAsync(episodeId, changeVoteDto, UserId, ct);
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
