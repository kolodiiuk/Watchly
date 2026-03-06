using Microsoft.AspNetCore.Identity;

namespace Watchly.Domain.Entities;

public sealed class User : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<WatchList> WatchLists { get; set; } = new List<WatchList>();

    public ICollection<UserTitleProgress> UserTitleProgresses { get; set; }
        = new List<UserTitleProgress>();

    public IEnumerable<UserContentActivity> UserContentActivities { get; set; }
        = new List<UserContentActivity>();
}
