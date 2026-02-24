using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class UserStatsController : BaseController<UserStatsController>
{
    public UserStatsController(ILogger<UserStatsController> logger) : base(logger)
    {
    }
}