using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Watchly.Api.Dto.Auth;
using Watchly.Api.Dto.UserProfile;
using Watchly.Api.Logging;
using Watchly.Application.Interfaces;
using Watchly.Application.Models;
using Watchly.Domain.Extensions;
using Watchly.Infrastructure.Interfaces;

namespace Watchly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UserProfileController : BaseController<UserProfileController>
{
    private readonly IUserManagementService _userManagementService;

    private readonly IPasswordManagementService _passwordManagementService;

    private readonly IImageService _imageService;

    public UserProfileController(IUserManagementService userManagementService,
        IPasswordManagementService passwordManagementService,
        IImageService imageService,
        ILogger<UserProfileController> logger)
        : base(logger)
    {
        _userManagementService = userManagementService;
        _passwordManagementService = passwordManagementService;
        _imageService = imageService;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("verify")]
    [EndpointSummary("Verifies the caller's JWT and returns profile data.")]
    [EndpointDescription(
        "Reads the user identifier from claims, loads the user entity, and confirms the token is still valid.")]
    public async Task<ActionResult<UserDto>> VerifyTokenAsync(CancellationToken ct)
    {
        Log(LogLevel.Information, AuthControllerEventIds.TokenVerificationAttempt,
            "Token verification attempt for user ID: {UserId}", UserId);
        ct.ThrowIfCancellationRequested();
        var result = await _userManagementService.GetUserAsync(UserId);
        if (result.Failure)
        {
            Log(LogLevel.Warning, AuthControllerEventIds.TokenVerificationFailed,
                "Token verification failed for user ID: {UserId}. Error: {Error}", UserId, result.Error);

            return Problem(
                title: "User verification failure",
                detail: result.Error,
                statusCode: StatusCodes.Status401Unauthorized);
        }

        Log(LogLevel.Information, AuthControllerEventIds.TokenVerifiedSuccess,
            "Successfully verified token for user ID: {UserId}", UserId);

        return StatusCode(StatusCodes.Status200OK, result.Value);
    }

    [EndpointSummary("Changes username.")]
    [EndpointDescription("Allows an authenticated user to change their username.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("change-username")]
    public async Task<IActionResult> ChangeUsernameAsync([FromBody] ChangeUserNameRequest req, CancellationToken ct)
    {
        Log(LogLevel.Information, AuthControllerEventIds.TokenVerificationAttempt,
            "Token verification attempt for user ID: {UserId}", UserId);
        ct.ThrowIfCancellationRequested();
        if (UserId == Guid.Empty || string.IsNullOrWhiteSpace(req.Name))
        {
            Log(LogLevel.Warning, AuthControllerEventIds.ChangeUserNameFailed,
                "User ID not found in claims or name is not valid");

            return Problem(
                title: "User ID or named validation failure",
                detail: "User ID or name is null or empty",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var res = await _userManagementService.ChangeUserNameAsync(UserId, req.Name);
        if (res.Failure)
        {
            Log(LogLevel.Warning, AuthControllerEventIds.ChangeUserNameFailed,
                "User name change failed: {error}", res.Error);

            return Problem(
                title: "User name change failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status200OK);
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPatch("change-password")]
    [EndpointSummary("Changes a password of existing user.")]
    [EndpointDescription("Accepts old password, new password from an authorized user.")]
    public async Task<IActionResult> ChangePasswordAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (req.OldPassword.IsNullOrEmpty() || req.NewPassword.IsNullOrEmpty()
                                            || req.NewPassword == req.OldPassword)
        {
            return Problem(
                title: "Invalid password data",
                detail: "Old password or new password is missing or are the same",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Log(LogLevel.Information, UserProfileControllerEventIds.ChangePasswordAttempt,
            "Start change password attempt for user {userId}", UserId);

        if (UserId == Guid.Empty)
        {
            return StatusCode(StatusCodes.Status401Unauthorized);
        }

        var res = await _passwordManagementService.ChangePasswordAsync(
            UserId, req.OldPassword, req.NewPassword);

        if (res.Failure)
        {
            Log(LogLevel.Error, UserProfileControllerEventIds.ChangePasswordFailure,
                "Problem changing password for user {userId}. Error: {err}",
                UserId, res.Error);
            if (res.Error.Contains("PasswordMismatch", StringComparison.CurrentCultureIgnoreCase))
            {
                return Problem(
                    title: "Invalid credentials",
                    detail: res.Error,
                    statusCode: StatusCodes.Status403Forbidden);
            }

            if (res.Error.Contains("user not found", StringComparison.CurrentCultureIgnoreCase))
            {
                return Problem(
                    title: "User not found",
                    detail: res.Error,
                    statusCode: StatusCodes.Status404NotFound);
            }

            if (res.Error.Contains("Password", StringComparison.CurrentCultureIgnoreCase))
            {
                return Problem(
                    title: "Invalid new password",
                    detail: res.Error,
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }

            return Problem(
                title: "Password change failed",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        Log(LogLevel.Information, UserProfileControllerEventIds.ChangePasswordSuccess,
            "Password was successfully changed for user {userId}", UserId);

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Changes a password of existing user.")]
    [EndpointDescription("Accepts old password, new password from an authorized user.")]
    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest req, CancellationToken ct)
    {
        if (req?.Email == null || !req.Email.IsValidEmail())
        {
            return Problem(
                title: "Not valid email",
                statusCode: StatusCodes.Status400BadRequest);
        }
        ct.ThrowIfCancellationRequested();
        var res = await _passwordManagementService.SendPasswordResetConfirmationAsync(req.Email, ct);
        if (res.Failure)
        {
            if (res.Error.Contains("Haven't found user with email"))
            {
                return Problem(
                    title: "User with email not found",
                    detail: res.Error,
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }
            else
            {
                return Problem(
                    title: "Problem while processing request",
                    detail: res.Error,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("reset-password")]
    [EndpointSummary("Validates a request for password reset from email.")]
    [EndpointDescription("Accepts token, validates it.")]
    public async Task<IActionResult> ResetPassword([FromQuery(Name = "token")] string token, CancellationToken ct)
    {
        if (token.IsNullOrEmpty())
        {
            return Problem(
                title: "Token is not provided",
                detail: "Token is not provided",
                statusCode: StatusCodes.Status400BadRequest);
        }
        ct.ThrowIfCancellationRequested();
        var res = await _passwordManagementService.ValidateResetPasswordRequestAsync(token, ct);
        if (res.Failure)
        {
            if (res.Error.Contains("Token doesn't match"))
            {
                return Problem(
                    title: "Password change is not authorised",
                    detail: res.Error,
                    statusCode: StatusCodes.Status403Forbidden);
            }
            else
            {
                return Problem(
                    title: "Problem while processing request",
                    detail: res.Error,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    [HttpPatch("profile-picture")]
    [EndpointSummary("Adds a new profile picture")]
    [EndpointDescription("If a user doesn't have one, adds it, otherwise overwrites existing url")]
    public async Task<IActionResult> UpdatePictureProfileAsync(
        IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return StatusCode(StatusCodes.Status400BadRequest, "No file uploaded.");
        }

        var imgUploadRes = await _imageService.SaveImageAsync(
            file.OpenReadStream(), file.FileName, ct);
        if (imgUploadRes.Failure)
        {
            return Problem(
                title: "Problem saving image",
                detail: imgUploadRes.Error,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var imgPersistRes = await _userManagementService.AddImageAsync(UserId, imgUploadRes.Value);
        if (imgPersistRes.Failure)
        {
            return Problem(
                title: "User ID or request validation failure",
                detail: "User ID or request is null or empty",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }
}
