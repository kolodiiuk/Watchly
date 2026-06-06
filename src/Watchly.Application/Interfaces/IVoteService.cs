using Watchly.Domain.Utils;
using Watchly.Application.Models.Votes;

namespace Watchly.Application.Interfaces;

public interface IVoteService
{
    Task<Result> VoteTitleAsync(int titleId, short value, Guid userId, CancellationToken ct);
    Task<Result> VoteEpisodeAsync(int episodeId, short value, Guid userId, CancellationToken ct);
    Task<Result> ChangeVoteAsync(int voteId, short value, Guid userId, CancellationToken ct);
    Task<Result<UserVote>> GetTitleVoteAsync(int titleId, Guid userId, CancellationToken ct);
    Task<Result<UserVote>> GetEpisodeVoteAsync(int episodeId, Guid userId, CancellationToken ct);
}
