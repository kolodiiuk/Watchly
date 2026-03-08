using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Vote;
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPost("title")]
    public async Task<IActionResult> VoteTitleAsync(int titleId, VoteDto voteDto, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Changes a vote on a title.")]
    [EndpointDescription("Updates a user's existing vote or rating for a specific title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpPut("title")]
    public async Task<IActionResult> ChangeVoteTitleAsync(int titleId, ChangeVoteDto changeVoteDto,
        CancellationToken ct)
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
    [HttpPut("{episodeId:int}")]
    public async Task<IActionResult> ChangeVoteAsync(int episodeId, ChangeVoteDto changeVoteDto,
        CancellationToken ct)
    {
        return StatusCode(418);
    }
}