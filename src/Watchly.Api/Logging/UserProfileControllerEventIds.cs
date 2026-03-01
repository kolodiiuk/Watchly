namespace Watchly.Api.Logging;

internal static class UserProfileControllerEventIds
{
    internal static readonly EventId ChangePasswordSuccess = new(1000, nameof(ChangePasswordSuccess));

    internal static readonly EventId ChangePasswordFailure = new(1001, nameof(ChangePasswordFailure));

    internal static readonly EventId ChangePasswordAttempt = new(1002, nameof(ChangePasswordAttempt));
}
