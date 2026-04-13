namespace Watchly.Api.Validators;

internal static class VoteValidator
{
    internal const string VoteTitle = "Watchly.Api.Controllers.VoteController.VoteTitleAsync (Watchly.Api)";
    internal const string ChangeVoteTitle = "Watchly.Api.Controllers.VoteController.ChangeVoteTitleAsync (Watchly.Api)";
    internal const string VoteEpisode = "Watchly.Api.Controllers.VoteController.VoteAsync (Watchly.Api)";
    internal const string ChangeVoteEpisode = "Watchly.Api.Controllers.VoteController.ChangeVoteAsync (Watchly.Api)";

    private const string TitleId = "titleId";
    private const string EpisodeId = "episodeId";
    private const string Value = "value";

    internal static bool ValidateVoteTitle(IDictionary<string, object> map)
    {
        return map.TryGetValue(TitleId, out var titleId)
               && titleId is int parsedTitleId
               && parsedTitleId >= 1
               && map.TryGetValue(Value, out var value)
               && IsVoteValueValid((short)value);
    }

    internal static bool ValidateChangeVoteTitle(IDictionary<string, object> map)
    {
        return map.TryGetValue(TitleId, out var titleId)
               && titleId is int parsedTitleId
               && parsedTitleId >= 1
               && map.TryGetValue(Value, out var value)
               && IsVoteValueValid((short)value);
    }

    internal static bool ValidateVoteEpisode(IDictionary<string, object> map)
    {
        return map.TryGetValue(EpisodeId, out var episodeId)
               && episodeId is int parsedEpisodeId
               && parsedEpisodeId >= 1
               && map.TryGetValue(Value, out var value)
               && IsVoteValueValid((short)value);
    }

    internal static bool ValidateChangeVoteEpisode(IDictionary<string, object> map)
    {
        return map.TryGetValue(EpisodeId, out var episodeId)
               && episodeId is int parsedEpisodeId
               && parsedEpisodeId >= 1
               && map.TryGetValue(Value, out var value)
               && IsVoteValueValid((short)value);
    }

    private static bool IsVoteValueValid(short v) => v >= 1 && v <= 10;
}
