using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TitleQueryingController : BaseController<TitleQueryingController>
{
    public TitleQueryingController(ILogger<TitleQueryingController> logger) : base(logger)
    {
    }
}
