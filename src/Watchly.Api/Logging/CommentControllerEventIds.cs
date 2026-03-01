namespace Watchly.Api.Logging;

internal static class CommentControllerEventIds
{
    internal static readonly EventId DeleteCommentFailed = new (2001, nameof(DeleteCommentFailed));

    internal static readonly EventId DeleteCommentAttempt = new(2002, nameof(DeleteCommentAttempt));

    internal static readonly EventId UpdateCommentAttempt = new(2003, nameof(UpdateCommentAttempt));

    internal static readonly EventId UpdateCommentFailed = new(2004, nameof(UpdateCommentFailed));

    internal static readonly EventId LeaveCommentFailed = new(2005, nameof(LeaveCommentFailed));

    internal static readonly EventId LeaveCommentAttempt = new(2006, nameof(LeaveCommentAttempt));

    internal static readonly EventId GetCommentsEpisodeFailed = new(2007, nameof(GetCommentsEpisodeFailed));

    internal static readonly EventId GetCommentsEpisodeAttempt = new(2008, nameof(GetCommentsEpisodeAttempt));

    internal static readonly EventId GetCommentsTitleFailed = new(2009, nameof(GetCommentsTitleFailed));

    internal static readonly EventId GetCommentsTitleAttempt = new(2010, nameof(GetCommentsTitleAttempt));
}
