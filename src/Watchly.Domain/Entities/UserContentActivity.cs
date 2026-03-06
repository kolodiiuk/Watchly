using Watchly.Domain.Enums;

namespace Watchly.Domain.Entities;

public sealed class UserContentActivity
{
    public int Id { get; set; }

    public int ContentId { get; set; }

    public Guid? UserId { get; set; }

    public ContentType ContentType { get; set; }

    public ActivityType ActivityType { get; set; }

    public DateTime WatchedAt { get; set; }

    public User User { get; set; }
}
