using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IVoteService
{
    Task<Result> VoteTitleAsync(int titleId, VoteDto voteDto, Guid result, CancellationToken ct);
    Task<Result> ChangeVoteTitleAsync(int titleId, ChangeVoteDto changeVoteDto, Guid result, CancellationToken ct);
    Task<Result> VoteEpisodeAsync(int episodeId, VoteDto voteDto, Guid result, CancellationToken ct);
    Task<Result> ChangeVoteEpisodeAsync(int episodeId, ChangeVoteDto changeVoteDto, Guid result, CancellationToken ct);
}