using Watchly.Application.Models.Content;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IContentService
{
    Task<Result<TitleInfo>> GetTitleByIdAsync(int titleId, CancellationToken ct);

    Task<Result<Episode>> GetEpisodeByIdAsync(int episodeId, CancellationToken ct);

    Task<Result<IEnumerable<TitleShortInfo>>> SearchTitlesAsync(string searchTerm, int pageSize, int page, CancellationToken ct);
    
    Task<Result<IEnumerable<TitleShortInfo>>> FilterTitlesAsync(
        FilterRequest filterOptions, CancellationToken ct);
}
