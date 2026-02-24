using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Vote;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TitleController : BaseController<TitleController>
{
    public TitleController(ILogger<TitleController> logger) : base(logger)
    {
    }
    
    [HttpPost("watch/{episodeId:int}")]
    public async Task<IActionResult> MarkWatchedAsync(int episodeId, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [HttpPost]
    public async Task<IActionResult> VoteAsync(int titleId, VoteDto voteDto, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [HttpPut]
    public async Task<IActionResult> ChangeVoteAsync(int titleId, ChangeVoteDto changeVoteDto,
        CancellationToken ct)
    {
        return StatusCode(418);
    }

    [HttpPost("comment")]
    public async Task<IActionResult> LeaveCommentAsync()
    {
        return StatusCode(418);
    }
}