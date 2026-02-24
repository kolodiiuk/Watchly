using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Vote;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class EpisodeController : BaseController<EpisodeController>
{
    public EpisodeController(ILogger<EpisodeController> logger) : base(logger)
    {
    }

    [HttpPost("watch/{episodeId:int}")]
    public async Task<IActionResult> MarkWatchedAsync(int episodeId, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [HttpPost("vote/{episodeId:int}")]
    public async Task<IActionResult> VoteAsync(int episodeId, VoteDto voteDto, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [HttpPut("vote/{episodeId:int}")]
    public async Task<IActionResult> ChangeVoteAsync(int episodeId, ChangeVoteDto changeVoteDto,
        CancellationToken ct)
    {
        return StatusCode(418);
    }

    [HttpPost("comment/{episodeId:int}")]
    public async Task<IActionResult> LeaveCommentAsync(int episodeId, string text, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [HttpPut("comment")]
    public async Task<IActionResult> UpdateCommentAsync(int commentId, string text, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [HttpDelete("comment/{commnentId:int}")]
    public async Task<IActionResult> DeleteCommentAsync(int commentId, CancellationToken ct)
    {
        return StatusCode(418);
    }
}
