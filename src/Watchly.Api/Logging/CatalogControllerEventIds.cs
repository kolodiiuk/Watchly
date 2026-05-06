namespace Watchly.Api.Logging;

internal static class CatalogControllerEventIds
{
    internal static readonly EventId SearchAttempt = new(5001, nameof(SearchAttempt));
    internal static readonly EventId SearchFailed = new(5002, nameof(SearchFailed));

    internal static readonly EventId FilterAttempt = new(5003, nameof(FilterAttempt));
    internal static readonly EventId FilterFailed = new(5004, nameof(FilterFailed));

    internal static readonly EventId GetTitleAttempt = new(5005, nameof(GetTitleAttempt));
    internal static readonly EventId GetTitleFailed = new(5006, nameof(GetTitleFailed));

    internal static readonly EventId GetEpisodeAttempt = new(5007, nameof(GetEpisodeAttempt));
    internal static readonly EventId GetEpisodeFailed = new(5008, nameof(GetEpisodeFailed));

    internal static readonly EventId GetKeywordSuggestionsFailed = new(5009, nameof(GetKeywordSuggestionsFailed));
    internal static readonly EventId GetSpokenLanguagesFailed = new(5010, nameof(GetSpokenLanguagesFailed));
}
