using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Comments;
using Watchly.Api.Filters;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Comments;

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
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> GetCommentsTitleAsync(int titleId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _commentService.GetCommentsAsync(titleId, true, ct);
        if (res.Failure)
        {
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
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> GetCommentsEpisodeAsync(int episodeId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _commentService.GetCommentsAsync(episodeId, false, ct);
        if (res.Failure)
        {
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
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> LeaveCommentAsync([FromBody] LeaveCommentRequest req, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        ct.ThrowIfCancellationRequested();
        var res = await _commentService.LeaveCommentAsync(req, UserId, ct);
        if (res.Failure)
        {
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
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> UpdateCommentAsync([FromBody] UpdateCommentRequest req, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var res = await _commentService.UpdateCommentAsync(req.CommentId, req.Text, UserId, ct);
        if (res.Failure)
        {
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
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<IActionResult> DeleteCommentAsync(int commentId, CancellationToken ct)
    {
        if (UserId == Guid.Empty)
        {
            return Unauthorized();
        }

        ct.ThrowIfCancellationRequested();
        var res = await _commentService.DeleteCommentAsync(commentId, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Comment deletion failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }
}
