using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class WatchListController : BaseController<WatchListController>
{
    public WatchListController(ILogger<WatchListController> logger) : base(logger)
    {
    }
}
