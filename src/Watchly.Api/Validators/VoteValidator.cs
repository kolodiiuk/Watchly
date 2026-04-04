using Watchly.Application.Interfaces;

namespace Watchly.Api.Validators;

internal static class VoteValidator
{
    internal const string VoteTitle = "Watchly.Api.Controllers.VoteController.VoteTitleAsync (Watchly.Api)";
    internal const string ChangeVoteTitle = "Watchly.Api.Controllers.VoteController.ChangeVoteTitleAsync (Watchly.Api)";
    internal const string VoteEpisode = "Watchly.Api.Controllers.VoteController.VoteAsync (Watchly.Api)";
    internal const string ChangeVoteEpisode = "Watchly.Api.Controllers.VoteController.ChangeVoteAsync (Watchly.Api)";

    internal const string TitleId = "titleId";
    internal const string EpisodeId = "episodeId";
    internal const string VoteDtoArgument = "voteDto";
    internal const string ChangeVoteDtoArgument = "changeVoteDto";

    internal static bool ValidateVoteTitle(IDictionary<string, object> map)
    {
        return map.TryGetValue(TitleId, out var titleId)
               && titleId is int parsedTitleId
               && parsedTitleId >= 1
               && map.TryGetValue(VoteDtoArgument, out var voteDto)
               && voteDto is VoteDto;
    }

    internal static bool ValidateChangeVoteTitle(IDictionary<string, object> map)
    {
        return map.TryGetValue(TitleId, out var titleId)
               && titleId is int parsedTitleId
               && parsedTitleId >= 1
               && map.TryGetValue(ChangeVoteDtoArgument, out var changeVoteDto)
               && changeVoteDto is ChangeVoteDto;
    }

    internal static bool ValidateVoteEpisode(IDictionary<string, object> map)
    {
        return map.TryGetValue(EpisodeId, out var episodeId)
               && episodeId is int parsedEpisodeId
               && parsedEpisodeId >= 1
               && map.TryGetValue(VoteDtoArgument, out var voteDto)
               && voteDto is VoteDto;
    }

    internal static bool ValidateChangeVoteEpisode(IDictionary<string, object> map)
    {
        return map.TryGetValue(EpisodeId, out var episodeId)
               && episodeId is int parsedEpisodeId
               && parsedEpisodeId >= 1
               && map.TryGetValue(ChangeVoteDtoArgument, out var changeVoteDto)
               && changeVoteDto is ChangeVoteDto;
    }
}
