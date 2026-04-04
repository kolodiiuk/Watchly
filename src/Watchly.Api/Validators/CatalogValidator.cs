namespace Watchly.Api.Validators;

internal static class CatalogValidator
{
    internal const string GetTitle = "Watchly.Api.Controllers.CatalogController.GetTitleAsync (Watchly.Api)";
    internal const string GetEpisode = "Watchly.Api.Controllers.CatalogController.GetEpisodeAsync (Watchly.Api)";
    internal const string Search = "Watchly.Api.Controllers.CatalogController.SearchAsync (Watchly.Api)";

    internal static bool ValidateGetTitle(IDictionary<string, object> map)
    {
        return map.TryGetValue("titleId", out var id) && (int)id >= 1;
    }

    internal static bool ValidateGetEpisode(IDictionary<string, object> map)
    {
        return map.TryGetValue("episodeId", out var id) && (int)id >= 1;
    }

    internal static bool ValidateSearch(IDictionary<string, object> map)
    {
        return map.TryGetValue("searchTerm", out var term) 
               && !string.IsNullOrWhiteSpace((string)term);
    }
}
