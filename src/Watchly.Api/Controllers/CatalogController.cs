using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Filters;
using Watchly.Api.Logging;
using Watchly.Application.Interfaces;
using Watchly.Application.Models;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CatalogController : BaseController<CatalogController>
{
    private readonly IContentService _contentService;

    public CatalogController(IContentService contentService, ILogger<CatalogController> logger)
        : base(logger)
    {
        _contentService = contentService;
    }

    [EndpointSummary("Searches for titles.")]
    [EndpointDescription(
        "Searches the catalog for titles matching the provided term (in title, description, keywords, etc.).")]
    [ProducesResponseType(typeof(IEnumerable<TitleShortInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ServiceFilter(typeof(ValidationFilter))]
    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TitleShortInfo>>> SearchAsync(
        [FromQuery(Name = "term")] string searchTerm,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _contentService.SearchTitlesAsync(searchTerm, ct);
        if (res.Failure)
        {
            Log(LogLevel.Error, CatalogControllerEventIds.SearchFailed, "Search for term {term} failed: {error}",
                searchTerm, res.Error);

            return Problem(
                title: "Search failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }

    [EndpointSummary("Filters titles.")]
    [EndpointDescription(
        "Returns a filtered list of titles based on search criteria (genre, keywords, production company, content type, etc.)")]
    [ProducesResponseType(typeof(IEnumerable<TitleShortInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<TitleShortInfo>>> FilterTitlesAsync(
        [FromQuery] FilterRequest filter,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _contentService.FilterTitlesAsync(filter, ct);
        if (res.Failure)
        {
            Log(LogLevel.Error, CatalogControllerEventIds.FilterFailed, 
                "Filter failed: {error}", res.Error);

            return Problem(
                title: "Filter failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }

    [EndpointSummary("Gets a specific title.")]
    [EndpointDescription("Retrieves detailed information about a title by its ID for detailed display.")]
    [ProducesResponseType(typeof(TitleInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [ServiceFilter(typeof(ValidationFilter))]
    [HttpGet("{titleId:int}")]
    public async Task<ActionResult<TitleInfo>> GetTitleAsync(int titleId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _contentService.GetTitleByIdAsync(titleId, ct);
        if (res.Failure)
        {
            Log(LogLevel.Error, CatalogControllerEventIds.GetTitleFailed, "Get title {titleId} failed: {error}",
                titleId, res.Error);

            return Problem(
                title: "Get title failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }

    [EndpointSummary("Gets a specific episode.")]
    [EndpointDescription(
        "Retrieves detailed information about an episode by its ID, season, and title for detailed display.")]
    [ProducesResponseType(typeof(EpisodeInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [ServiceFilter(typeof(ValidationFilter))]
    [HttpGet("episode/{episodeId:int}")]
    public async Task<ActionResult<EpisodeInfo>> GetEpisodeAsync(int episodeId, CancellationToken ct)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}

public record TitleInfo(
    int Id,
    string Name,
    string Overview,
    string PosterUrl,
    DateTime? ReleaseDate,
    int Runtime,
    float? AvgTmdbRating);

public record EpisodeInfo(int TitleId, int SeasonId, int EpisodeId);
