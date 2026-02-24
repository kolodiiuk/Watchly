using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UserProfileController : BaseController<UserProfileController>
{
    public UserProfileController(ILogger<UserProfileController> logger) : base(logger)
    {
    }

    [EndpointSummary("Changes username.")]
    [EndpointDescription("Allows an authenticated user to change their username.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("change-username")]
    public async Task<IActionResult> ChangeUsernameAsync(string name, CancellationToken ct)
    {
        return StatusCode(418);
    }
}
