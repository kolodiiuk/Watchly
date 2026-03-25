using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Logging;
using Watchly.Application.Interfaces;
using Watchly.Domain.Entities;

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
    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TitleShortInfo>>> SearchAsync(
        [FromQuery(Name = "term")] string searchTerm,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Problem(
                title: "Invalid search term",
                detail: "Search term must not be empty.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, CatalogControllerEventIds.SearchAttempt, "Search attempt for term {term}",
            searchTerm);

        // prepare predicate (case-insensitive)
        bool Predicate(Title t) =>
            (!string.IsNullOrEmpty(t.Name) && t.Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase))
            || (!string.IsNullOrEmpty(t.Overview) &&
                t.Overview.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase));

        var res = await _contentService.GetTitlesByConditionAsync(Predicate, ct);
        if (res.Failure)
        {
            Log(LogLevel.Error, CatalogControllerEventIds.SearchFailed, "Search for term {term} failed: {error}",
                searchTerm, res.Error);

            return Problem(
                title: "Search failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        var mapped = res.Value.Select(t => new TitleShortInfo(
            t.Id,
            t.Name,
            t.PosterUrl,
            t.AvgTmdbRating
        ));

        return StatusCode(StatusCodes.Status200OK, mapped);
    }

    [EndpointSummary("Filters titles.")]
    [EndpointDescription(
        "Returns a filtered list of titles based on search criteria (genre, keywords, production company, content type, etc.)")]
    [ProducesResponseType(typeof(IEnumerable<TitleShortInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<TitleShortInfo>>> FilterTitlesAsync([FromQuery] FilterRequest filter,
        CancellationToken ct)
    {
        // If no filter provided, return all (or you can choose to require some filter fields)
        filter ??= new FilterRequest();

        Log(LogLevel.Information, CatalogControllerEventIds.FilterAttempt, "Filter attempt {@filter}", filter);

        // NOTE: FilterRequest currently has no fields in your file. For now use a permissive predicate.
        // If you add fields to FilterRequest (e.g. Genre, ContentType), update this predicate to reflect them.
        bool Predicate(Title t) => true;

        var res = await _contentService.GetTitlesByConditionAsync(Predicate, ct);
        if (res.Failure)
        {
            Log(LogLevel.Error, CatalogControllerEventIds.FilterFailed, "Filter failed: {error}", res.Error);

            return Problem(
                title: "Filter failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        var mapped = res.Value.Select(t => new TitleShortInfo(
            t.Id,
            t.Name,
            t.PosterUrl,
            t.AvgTmdbRating
        ));

        return StatusCode(StatusCodes.Status200OK, mapped);
    }

    [EndpointSummary("Gets a specific title.")]
    [EndpointDescription("Retrieves detailed information about a title by its ID for detailed display.")]
    [ProducesResponseType(typeof(TitleInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [HttpGet("{titleId:int}")]
    public async Task<ActionResult<TitleInfo>> GetTitleAsync(int titleId, CancellationToken ct)
    {
        if (titleId < 1)
        {
            return Problem(
                title: "Invalid title id",
                detail: "Title id must be greater than 0.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, CatalogControllerEventIds.GetTitleAttempt, "Get title attempt for id {titleId}",
            titleId);

        // NOTE: your current `IContentService.GetTitleByIdAsync` signature (in repo) does not accept an id.
        // The controller calls it per the interface in your project. If you update the service to accept an id,
        // change call below to: await _contentService.GetTitleByIdAsync(titleId, ct);
        var res = await _contentService.GetTitleByIdAsync(ct);

        if (res.Failure)
        {
            Log(LogLevel.Error, CatalogControllerEventIds.GetTitleFailed, "Get title {titleId} failed: {error}",
                titleId, res.Error);

            return Problem(
                title: "Get title failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        var t = res.Value;
        var info = new TitleInfo(
            t.Id,
            t.Name,
            t.Overview,
            t.PosterUrl,
            t.ReleaseDate,
            t.Runtime,
            t.AvgTmdbRating
        );

        return StatusCode(StatusCodes.Status200OK, info);
    }

    [EndpointSummary("Gets a specific episode.")]
    [EndpointDescription(
        "Retrieves detailed information about an episode by its ID, season, and title for detailed display.")]
    [ProducesResponseType(typeof(EpisodeInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [HttpGet("{titleId:int}/{seasonId:int}/{episodeId:int}")]
    public async Task<ActionResult<EpisodeInfo>> GetEpisodeAsync(
        int titleId, int seasonId, int episodeId,
        CancellationToken ct)
    {
        if (titleId < 1 || seasonId < 1 || episodeId < 1)
        {
            return Problem(
                title: "Invalid parameters",
                detail: "IDs must be greater than 0.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, CatalogControllerEventIds.GetEpisodeAttempt,
            "Get episode attempt for title {titleId} season {seasonId} episode {episodeId}", titleId, seasonId,
            episodeId);

        // No dedicated episode retrieval exists on IContentService in current repo.
        // If you implement such a method, call it here and map the result. For now return NotImplemented:
        Log(LogLevel.Warning, CatalogControllerEventIds.GetEpisodeFailed,
            "Get episode not implemented for title {titleId} season {seasonId} episode {episodeId}", titleId, seasonId,
            episodeId);

        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}

/* DTO-like records local to controller file for simple mapping.
   Adjust fields as needed, or replace with project-wide DTO types. */

public record TitleShortInfo(int Id, string Name, string PosterUrl, float? AvgTmdbRating);

public record TitleInfo(
    int Id,
    string Name,
    string Overview,
    string PosterUrl,
    DateTime? ReleaseDate,
    int Runtime,
    float? AvgTmdbRating);

public record EpisodeInfo(int TitleId, int SeasonId, int EpisodeId);

public record FilterRequest
{
    // Add fields (Genre, ContentType, Keywords, etc.) as needed for filtering.
}
