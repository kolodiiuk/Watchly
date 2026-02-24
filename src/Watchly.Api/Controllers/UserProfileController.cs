using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class UserProfileController : BaseController<UserProfileController>
{
    public UserProfileController(ILogger<UserProfileController> logger) : base(logger)
    {
    }

    [HttpPost("change-username")]
    public async Task<IActionResult> ChangeUsernameAsync(string name, CancellationToken ct)
    {
        return StatusCode(418);
    }
}
