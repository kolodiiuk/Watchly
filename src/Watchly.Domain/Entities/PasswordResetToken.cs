namespace Watchly.Domain.Entities;

public sealed class PasswordResetToken
{
    public int Id { get; set; }

    public DateTime Expires { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; }
}
