using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.AdminContent;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public sealed class AdminContentController : BaseController<AdminContentController>
{
    private readonly IAdminContentService _adminContentService;

    public AdminContentController(IAdminContentService adminContentService, ILogger<AdminContentController> logger)
        : base(logger)
    {
        _adminContentService = adminContentService;
    }

    [HttpPost("titles")]
    public async Task<IActionResult> AddTitleAsync([FromBody] CreateTitleRequest request, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _adminContentService.AddTitleAsync(request, ct);
        if (res.Failure)
        {
            return Problem(title: "Add title failed", detail: res.Error,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return StatusCode(StatusCodes.Status201Created, res.Value);
    }

    [HttpPut("titles/{titleId:int}")]
    public async Task<IActionResult> UpdateTitleAsync(int titleId, [FromBody] UpdateTitleRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _adminContentService.UpdateTitleAsync(titleId, request, ct);
        if (res.Failure)
        {
            return Problem(title: "Update title failed", detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPost("titles/{titleId:int}/poster")]
    public async Task<IActionResult> UploadPosterAsync(int titleId, [FromForm] IFormFile poster,
        CancellationToken ct = default)
    {
        if (poster.Length == 0)
        {
            return Problem(title: "Upload poster failed", detail: "Poster file is empty",
                statusCode: StatusCodes.Status400BadRequest);
        }

        await using var stream = poster.OpenReadStream();
        var res = await _adminContentService.UploadPosterAsync(titleId, stream, ct);
        if (res.Failure)
        {
            return Problem(title: "Upload poster failed", detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPatch("titles/{titleId:int}/delete")]
    public async Task<IActionResult> SoftDeleteTitleAsync(int titleId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _adminContentService.SoftDeleteTitleAsync(titleId, ct);
        if (res.Failure)
        {
            return Problem(title: "Soft delete title failed", detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPost("titles/{titleId:int}/seasons")]
    public async Task<IActionResult> AddSeasonAsync(int titleId, [FromBody] CreateSeasonRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _adminContentService.AddSeasonAsync(titleId, request, ct);
        if (res.Failure)
        {
            return Problem(title: "Add season failed", detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status201Created, res.Value);
    }

    [HttpPut("seasons/{seasonId:int}")]
    public async Task<IActionResult> UpdateSeasonAsync(int seasonId, [FromBody] UpdateSeasonRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _adminContentService.UpdateSeasonAsync(seasonId, request, ct);
        if (res.Failure)
        {
            return Problem(title: "Update season failed", detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("seasons/{seasonId:int}")]
    public async Task<IActionResult> RemoveSeasonAsync(int seasonId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _adminContentService.RemoveSeasonAsync(seasonId, ct);
        if (res.Failure)
        {
            return Problem(title: "Remove season failed", detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPost("seasons/{seasonId:int}/episodes")]
    public async Task<IActionResult> AddEpisodeAsync(int seasonId, [FromBody] CreateEpisodeRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _adminContentService.AddEpisodeAsync(seasonId, request, ct);
        if (res.Failure)
        {
            return Problem(title: "Add episode failed", detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status201Created, res.Value);
    }

    [HttpPut("episodes/{episodeId:int}")]
    public async Task<IActionResult> UpdateEpisodeAsync(int episodeId, [FromBody] UpdateEpisodeRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _adminContentService.UpdateEpisodeAsync(episodeId, request, ct);
        if (res.Failure)
        {
            return Problem(title: "Update episode failed", detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("episodes/{episodeId:int}")]
    public async Task<IActionResult> RemoveEpisodeAsync(int episodeId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _adminContentService.RemoveEpisodeAsync(episodeId, ct);
        if (res.Failure)
        {
            return Problem(title: "Remove episode failed", detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }
}
