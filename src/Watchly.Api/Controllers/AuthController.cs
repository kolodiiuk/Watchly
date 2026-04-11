using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Auth;
using Watchly.Api.Filters;
using Watchly.Api.Logging;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Auth;

namespace Watchly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : BaseController<AuthController>
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService, ILogger<AuthController> logger) :
        base(logger)
    {
        _authService = authService;
    }

    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("sign-up")]
    [ServiceFilter(typeof(ValidationFilter))]
    [EndpointSummary("Registers a new user account.")]
    [EndpointDescription(
        "Validates the incoming registration payload and creates a user with the provided credentials.")]
    public async Task<IActionResult> SignUpAsync(SignUpRequest signUpRequest, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = await _authService.SignUpAsync(signUpRequest.Email, signUpRequest.Password);
        if (result.Failure)
        {
            Log(LogLevel.Error, AuthControllerEventIds.SignUpFailed,
                "Sign up failed for email: {Email}. Error: {Error}",
                signUpRequest.Email, result.Error);

            return Problem(
                title: "Server error",
                detail: result.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status201Created);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("sign-in")]
    [ServiceFilter(typeof(ValidationFilter))]
    [EndpointSummary("Authenticates a user with email and password.")]
    [EndpointDescription("Validates user credentials and returns access plus tokens.")]
    public async Task<ActionResult<SignInResponse>> SignInAsync(SignInRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var response = await _authService.SignInAsync(request.Email, request.Password, IpAddress, ct);
        if (response.Failure)
        {
            Log(LogLevel.Warning, AuthControllerEventIds.SignInFailed,
                "Sign in failed: {Error}", response.Error);

            return Problem(
                title: "Sign in failed",
                detail: response.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status200OK, response.Value);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("refresh")]
    [ServiceFilter(typeof(ValidationFilter))]
    [EndpointSummary("Refreshes an access token using a refresh token.")]
    [EndpointDescription(
        "Validates the supplied refresh token, regenerates JWT credentials, and returns updated token metadata.")]
    public async Task<ActionResult<SignInResponse>> RefreshAsync([FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var res = await _authService.RefreshTokenAsync(request.RefreshToken, IpAddress, ct);
        if (res.Failure)
        {
            Log(LogLevel.Warning, AuthControllerEventIds.TokenRefreshFailed,
                "Token refresh failed: {Error}", res.Error);

            return Problem(
                title: "Failure refreshing token",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("sign-out")]
    [ServiceFilter(typeof(ValidationFilter))]
    [EndpointSummary("Signs out a user by revoking the refresh token.")]
    [EndpointDescription("Ensures a refresh token is provided and invalidates it to end the user session.")]
    public async Task<IActionResult> SignOutAsync([FromBody] SignOutDto request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = await _authService.SignOutAsync(request.RefreshToken, IpAddress, ct);
        if (result.Failure)
        {
            Log(LogLevel.Warning, AuthControllerEventIds.SignOutFailed,
                "Sign out failed: {Error}", result.Error);

            return Problem(
                title: "Sign out failure",
                detail: result.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status200OK, new { message = "Signed out successfully" });
    }
}
