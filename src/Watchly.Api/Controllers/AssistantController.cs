using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Comments;
using Watchly.Api.Filters;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Comments;
using Watchly.Application.Services;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AssistantController : BaseController<AssistantController>
{
    private readonly IAssistantService _assistantService;

    public AssistantController(IAssistantService commentService, ILogger<AssistantController> logger)
        : base(logger)
    {
        _assistantService = commentService;
    }

    [EndpointSummary("Gets relevant comments of a title.")]
    [EndpointDescription("Retrieves relevant comments for a specified title based on user's request.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [HttpPost("relevant/{titleId:int}")]
    public async Task<IActionResult> GetRelevantCommentInfoTitleAsync(int titleId, [FromBody] RelevantCommentRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _assistantService.GetRelevantCommentInfoAsync(titleId, true, request, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Get relevant comments for title failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }

    [EndpointSummary("Gets relevant comments of an episode.")]
    [EndpointDescription("Retrieves relevant comments for a specified episode based on user's request.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [HttpPost("relevant/episode/{episodeId:int}")]
    public async Task<IActionResult> GetRelevantCommentInfoEpisodeAsync(int episodeId, [FromBody] RelevantCommentRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = await _assistantService.GetRelevantCommentInfoAsync(episodeId, false, request, UserId, ct);
        if (res.Failure)
        {
            return Problem(
                title: "Get relevant comments for title failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }
}
