using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class EpisodeQueryingController : BaseController<EpisodeQueryingController>
{
    public EpisodeQueryingController(ILogger<EpisodeQueryingController> logger) : base(logger)
    {
    }

    [HttpGet("{episodeId:int}")]
    public async Task<IActionResult> GetEpisodeInfoAsync(int episodeId, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [HttpGet("season/{seasonId:int}")]
    public async Task<IActionResult> GetEpisodeInfoPerSeasonAsync(int seasonId, CancellationToken ct)
    {
        return StatusCode(418);
    }
}
