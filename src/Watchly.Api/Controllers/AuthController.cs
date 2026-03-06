using Microsoft.AspNetCore.Mvc;
using Watchly.Api.Dto.Auth;
using Watchly.Api.Logging;
using Watchly.Application.Interfaces;
using Watchly.Application.Models;

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

    private string IpAddress =>
        HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";

    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("sign-up")]
    [EndpointSummary("Registers a new user account.")]
    [EndpointDescription(
        "Validates the incoming registration payload and creates a user with the provided credentials.")]
    public async Task<IActionResult> SignUpAsync(SignUpRequest signUpRequest, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Log(LogLevel.Information, AuthControllerEventIds.SignUpAttempt,
            "Signup attempt for email: {Email}", signUpRequest?.Email);

        if (signUpRequest == null || !signUpRequest.IsValid())
        {
            Log(LogLevel.Warning, AuthControllerEventIds.SignUpInvalidNull,
                "Invalid sign up data: ");

            return Problem(
                title: "Invalid sign up data",
                detail: "Request is null or not valid fields",
                statusCode: StatusCodes.Status400BadRequest);
        }

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

        Log(LogLevel.Information, AuthControllerEventIds.SignUpSuccess,
            "Successfully registered user with email: {Email}", signUpRequest.Email);

        return StatusCode(StatusCodes.Status201Created);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("sign-in")]
    [EndpointSummary("Authenticates a user with email and password.")]
    [EndpointDescription("Validates user credentials and returns access plus tokens.")]
    public async Task<ActionResult<SignInResponse>> SignInAsync(SignInRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Log(LogLevel.Information, AuthControllerEventIds.SignInAttempt,
            "Sign in attempt for email: {Email}", request?.Email);

        if (request == null || !request.IsValid())
        {
            Log(LogLevel.Warning, AuthControllerEventIds.SignInInvalidNull,
                "Invalid sign in data: request is null");

            return Problem(
                title: "Invalid sign in user data",
                detail: "Request is null or not valid fields",
                statusCode: StatusCodes.Status400BadRequest);
        }

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

        Log(LogLevel.Information, AuthControllerEventIds.SignInSuccess,
            "Successfully signed in user: {Email}", request.Email);

        return StatusCode(StatusCodes.Status200OK, response.Value);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("refresh")]
    [EndpointSummary("Refreshes an access token using a refresh token.")]
    [EndpointDescription(
        "Validates the supplied refresh token, regenerates JWT credentials, and returns updated token metadata.")]
    public async Task<ActionResult<SignInResponse>> RefreshAsync([FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Log(LogLevel.Information, AuthControllerEventIds.TokenRefreshAttempt, "Token refresh attempt");

        if (request == null || string.IsNullOrEmpty(request.RefreshToken))
        {
            Log(LogLevel.Warning, AuthControllerEventIds.TokenRefreshEmpty, "Refresh token is empty");

            return Problem(
                title: "Invalid refresh token",
                detail: "Request is null or not valid fields",
                statusCode: StatusCodes.Status400BadRequest);
        }

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

        Log(LogLevel.Information, AuthControllerEventIds.TokenRefreshedSuccess,
            "Successfully refreshed token for user ID: {UserId}", res.Value.Id);

        return StatusCode(StatusCodes.Status200OK, res.Value);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("sign-out")]
    [EndpointSummary("Signs out a user by revoking the refresh token.")]
    [EndpointDescription("Ensures a refresh token is provided and invalidates it to end the user session.")]
    public async Task<IActionResult> SignOutAsync([FromBody] SignOutDto request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Log(LogLevel.Information, AuthControllerEventIds.SignOutAttempt, "Sign out attempt");

        if (request == null || string.IsNullOrEmpty(request.RefreshToken))
        {
            Log(LogLevel.Warning, AuthControllerEventIds.SignOutEmptyToken,
                "Sign out failed: refresh token is empty");

            return Problem(
                title: "Refresh token validation failure",
                detail: "Request is null or token is null or empty",
                statusCode: StatusCodes.Status400BadRequest);
        }

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

        Log(LogLevel.Information, AuthControllerEventIds.SignOutSuccess,
            "Successfully signed out user");

        return StatusCode(StatusCodes.Status200OK, new { message = "Signed out successfully" });
    }
}
