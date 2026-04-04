namespace Watchly.Api.Logging;

internal static class VoteControllerEventIds
{
    internal static readonly EventId VoteTitleFailed = new(3001, nameof(VoteTitleFailed));
    internal static readonly EventId VoteTitleAttempt = new(3002, nameof(VoteTitleAttempt));

    internal static readonly EventId ChangeVoteTitleAttempt = new(3003, nameof(ChangeVoteTitleAttempt));
    internal static readonly EventId ChangeVoteTitleFailed = new(3004, nameof(ChangeVoteTitleFailed));

    internal static readonly EventId VoteEpisodeAttempt = new(3005, nameof(VoteEpisodeAttempt));
    internal static readonly EventId VoteEpisodeFailed = new(3006, nameof(VoteEpisodeFailed));

    internal static readonly EventId ChangeVoteEpisodeAttempt = new(3007, nameof(ChangeVoteEpisodeAttempt));
    internal static readonly EventId ChangeVoteEpisodeFailed = new(3008, nameof(ChangeVoteEpisodeFailed));
}
