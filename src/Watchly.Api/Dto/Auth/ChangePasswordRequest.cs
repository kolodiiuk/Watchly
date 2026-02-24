namespace Watchly.Api.Dto.Auth;

public record ChangePasswordRequest(string OldPassword, string NewPassword);
