using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CatalogController : BaseController<CatalogController>
{
    public CatalogController(ILogger<CatalogController> logger) : base(logger)
    {
    }

    [EndpointSummary("Searches for titles.")]
    [EndpointDescription(
        "Searches the catalog for titles matching the provided term (in title, description, keywords, etc.).")]
    [ProducesResponseType(typeof(IEnumerable<TitleShortInfo>), StatusCodes.Status200OK)]
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TitleShortInfo>>> SearchAsync(
        [FromQuery(Name = "term")] string searchTerm,
        CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Filters titles.")]
    [EndpointDescription(
        "Returns a filtered list of titles based on search criteria (genre, keywords, production company, content type, etc..")]
    [ProducesResponseType(typeof(IEnumerable<TitleShortInfo>), StatusCodes.Status200OK)]
    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<TitleShortInfo>>> FilterTitlesAsync(CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Gets a specific title.")]
    [EndpointDescription("Retrieves detailed information about a title by its ID for detailed display.")]
    [ProducesResponseType(typeof(TitleInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{titleId:int}")]
    public async Task<ActionResult<TitleInfo>> GetTitleAsync(int titleId, CancellationToken ct)
    {
        return StatusCode(418);
    }

    [EndpointSummary("Gets a specific episode.")]
    [EndpointDescription(
        "Retrieves detailed information about an episode by its ID, season, and title for detailed display.")]
    [ProducesResponseType(typeof(EpisodeInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{titleId:int}/{seasonId:int}/{episodeId:int}")]
    public async Task<ActionResult<EpisodeInfo>> GetEpisodeAsync(
        int titleId, int seasonId, int episodeId,
        CancellationToken ct)
    {
        return StatusCode(418);
    }
}

public record EpisodeInfo
{
}

public record FilterRequest
{
}

public record TitleInfo
{
}

public record TitleShortInfo
{
}
