using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Comments;
using Watchly.Api.Logging;
using Watchly.Application.Interfaces;
using Watchly.Application.Models;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CommentController : BaseController<CommentController>
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService, ILogger<CommentController> logger)
        : base(logger)
    {
        _commentService = commentService;
    }

    [EndpointSummary("Gets comments for a title.")]
    [EndpointDescription("Retrieves all comments associated with the specified title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [HttpGet("title/{titleId:int}")]
    public async Task<IActionResult> GetCommentsTitleAsync(int titleId, CancellationToken ct)
    {
        if (titleId < 1)
        {
            return Problem(
                title: "Invalid title id",
                detail: "Title id is less than 1",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, CommentControllerEventIds.GetCommentsTitleAttempt,
            "Get comments attempt for title {id}", titleId);
        var res = await _commentService.GetCommentsAsync(titleId, true, ct);
        if (res.Failure)
        {
            Log(LogLevel.Error, CommentControllerEventIds.GetCommentsTitleFailed,
                "Get comments for title {titleId} failed: {error}", titleId, res.Error);

            return Problem(
                title: "Get comment for title failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }

    [EndpointSummary("Gets comments for a title.")]
    [EndpointDescription("Retrieves all comments associated with the specified title.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [HttpGet("episode/{episodeId:int}")]
    public async Task<IActionResult> GetCommentsEpisodeAsync(int episodeId, CancellationToken ct)
    {
        if (episodeId < 1)
        {
            return Problem(
                title: "Invalid episode id",
                detail: "Episode id is less than 1",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, CommentControllerEventIds.GetCommentsEpisodeAttempt,
            "Get comments attempt for episode {id}", episodeId);
        var res = await _commentService.GetCommentsAsync(episodeId, false, ct);
        if (res.Failure)
        {
            Log(LogLevel.Error, CommentControllerEventIds.GetCommentsEpisodeFailed,
                "Get comments for episode {id} failed {error}", episodeId, res.Error);

            return Problem(
                title: "Get comments failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }

    [EndpointSummary("Leaves a comment on a title or episode.")]
    [EndpointDescription("Adds a new user comment to the specified title or episode.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> LeaveCommentAsync([FromBody] LeaveCommentRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isParsed = Guid.TryParse(userId, out var guid);
        if (!isParsed || string.IsNullOrWhiteSpace(req.Text) || req.ContentId < 1)
        {
            Log(LogLevel.Warning, CommentControllerEventIds.LeaveCommentFailed,
                "User ID not found in claims or request is not valid");

            return Problem(
                title: "User ID or request validation failure",
                detail: "User ID or request is null or empty",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, CommentControllerEventIds.LeaveCommentAttempt,
            "Leave comment attempt for user ID: {UserId}", userId);
        var res = await _commentService.LeaveCommentAsync(req, guid, ct);
        if (res.Failure)
        {
            Log(LogLevel.Warning, CommentControllerEventIds.LeaveCommentFailed,
                "Leave comment failed: {error}", res.Error);

            return Problem(
                title: "Leave comment failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [EndpointSummary("Updates a comment.")]
    [EndpointDescription("Modifies an existing comment by its ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> UpdateCommentAsync([FromBody] UpdateCommentRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isParsed = Guid.TryParse(userId, out var guid);
        if (!isParsed || string.IsNullOrWhiteSpace(req.Text) || req.CommentId < 1)
        {
            Log(LogLevel.Warning, CommentControllerEventIds.UpdateCommentFailed,
                "User ID not found in claims or request is not valid");

            return Problem(
                title: "User ID not found in claims or request is not valid",
                detail: "User ID not found in claims or request is not valid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, CommentControllerEventIds.UpdateCommentAttempt,
            "Comment {comment} update attempt for user ID: {UserId}", req.CommentId, userId);
        var res = await _commentService.UpdateCommentAsync(req.CommentId, req.Text, guid, ct);
        if (res.Failure)
        {
            Log(LogLevel.Warning, CommentControllerEventIds.UpdateCommentFailed,
                "Comment {comment} update failed: {error}", req.CommentId, res.Error);

            return Problem(
                title: "Comment update failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [EndpointSummary("Deletes a comment.")]
    [EndpointDescription("Removes a user comment by its ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPatch("{commentId:int}")]
    public async Task<IActionResult> DeleteCommentAsync(int commentId, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isParsed = Guid.TryParse(userId, out var guid);
        if (!isParsed || commentId < 0)
        {
            Log(LogLevel.Warning, CommentControllerEventIds.DeleteCommentFailed,
                "User ID not found in claims or comment ID is not valid");

            return Problem(
                title: "User ID or comment ID validation failure",
                detail: "User ID or comment ID is null or empty",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, CommentControllerEventIds.DeleteCommentAttempt,
            "Comment {comment} deletion attempt for user ID: {UserId}", commentId, userId);
        var res = await _commentService.DeleteCommentAsync(commentId, guid, ct);
        if (res.Failure)
        {
            Log(LogLevel.Warning, CommentControllerEventIds.DeleteCommentFailed,
                "Comment deletion failed: {error}", res.Error);

            return Problem(
                title: "Comment deletion failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }
}

