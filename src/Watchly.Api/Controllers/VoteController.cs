using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Logging;
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
    public async Task<IActionResult> VoteTitleAsync(int titleId, VoteDto voteDto, CancellationToken ct)
    {
        if (titleId < 1 || voteDto is null)
        {
            Log(LogLevel.Warning, VoteControllerEventIds.VoteTitleFailed,
                "User ID not found in claims or request is not valid");

            return Problem(
                title: "User ID or request validation failure",
                detail: "User ID or request is null or invalid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, VoteControllerEventIds.VoteTitleAttempt,
            "Vote attempt for title {titleId} by user {UserId}", titleId, UserId);

        var res = await _voteService.VoteTitleAsync(titleId, voteDto, UserId, ct);
        if (res.Failure)
        {
            Log(LogLevel.Warning, VoteControllerEventIds.VoteTitleFailed,
                "Vote for title {titleId} failed: {error}", titleId, res.Error);

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
    public async Task<IActionResult> ChangeVoteTitleAsync(int titleId, ChangeVoteDto changeVoteDto,
        CancellationToken ct)
    {
        if (UserId != Guid.Empty || titleId < 1 || changeVoteDto is null)
        {
            Log(LogLevel.Warning, VoteControllerEventIds.ChangeVoteTitleFailed,
                "User ID not found in claims or request is not valid");

            return Problem(
                title: "User ID or request validation failure",
                detail: "User ID or request is null or invalid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, VoteControllerEventIds.ChangeVoteTitleAttempt,
            "Change vote attempt for title {titleId} by user {UserId}", titleId, UserId);

        var res = await _voteService.ChangeVoteTitleAsync(titleId, changeVoteDto, UserId, ct);
        if (res.Failure)
        {
            Log(LogLevel.Warning, VoteControllerEventIds.ChangeVoteTitleFailed,
                "Change vote for title {titleId} failed: {error}", titleId, res.Error);

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
    public async Task<IActionResult> VoteAsync(int episodeId, VoteDto voteDto, CancellationToken ct)
    {
        if (UserId != Guid.Empty || episodeId < 1 || voteDto is null)
        {
            Log(LogLevel.Warning, VoteControllerEventIds.VoteEpisodeFailed,
                "User ID not found in claims or request is not valid");

            return Problem(
                title: "User ID or request validation failure",
                detail: "User ID or request is null or invalid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, VoteControllerEventIds.VoteEpisodeAttempt,
            "Vote attempt for episode {episodeId} by user {UserId}", episodeId, UserId);

        var res = await _voteService.VoteEpisodeAsync(episodeId, voteDto, UserId, ct);
        if (res.Failure)
        {
            Log(LogLevel.Warning, VoteControllerEventIds.VoteEpisodeFailed,
                "Vote for episode {episodeId} failed: {error}", episodeId, res.Error);

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
    public async Task<IActionResult> ChangeVoteAsync(int episodeId, ChangeVoteDto changeVoteDto,
        CancellationToken ct)
    {
        if (UserId != Guid.Empty || episodeId < 1 || changeVoteDto is null)
        {
            Log(LogLevel.Warning, VoteControllerEventIds.ChangeVoteEpisodeFailed,
                "User ID not found in claims or request is not valid");

            return Problem(
                title: "User ID or request validation failure",
                detail: "User ID or request is null or invalid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, VoteControllerEventIds.ChangeVoteEpisodeAttempt,
            "Change vote attempt for episode {episodeId} by user {UserId}", episodeId, UserId);

        var res = await _voteService.ChangeVoteEpisodeAsync(episodeId, changeVoteDto, UserId, ct);
        if (res.Failure)
        {
            Log(LogLevel.Warning, VoteControllerEventIds.ChangeVoteEpisodeFailed,
                "Change vote for episode {episodeId} failed: {error}", episodeId, res.Error);

            return Problem(
                title: "Change vote failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }
}
