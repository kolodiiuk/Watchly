using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchly.Application.Interfaces;

namespace Watchly.Api.Controllers;

[ApiController]
//[Authorize]
[Route("api/[controller]")]
public class DataImportController : BaseController<AssistantController>
{
    private readonly IDataImportService _dataImportService;

    public DataImportController(IDataImportService dataImportService, ILogger<AssistantController> logger)
        : base(logger)
    {
        _dataImportService = dataImportService;
    }

    [EndpointSummary("Import additional data on a title.")]
    [EndpointDescription("Sets actors, director and isAdult for a title based on a TMDB api response.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("import/{titleId:int}")]
    public async Task<IActionResult> ImportDataForTitleAsync(int titleId)
    {
        var res = await _dataImportService.ImportTmdbDataAsync(titleId);
        if (res.Failure)
        {
            return Problem(
                title: "Import data for title failed",
                detail: res.Error,
                statusCode: StatusCodes.Status404NotFound);
        }

        return StatusCode(StatusCodes.Status200OK);
    }
}
