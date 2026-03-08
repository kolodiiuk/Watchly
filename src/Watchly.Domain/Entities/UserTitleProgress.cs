namespace Watchly.Domain.Entities;

public class UserTitleProgress
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int TitleId { get; set; }

    public WatchStatus Status { get; set; }

    public Title Title { get; set; }

    public User User { get; set; }
}
