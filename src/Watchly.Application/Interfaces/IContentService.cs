using Watchly.Application.Models;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IContentService
{
    Task<Result<Title>> GetTitleByIdAsync(int titleId, CancellationToken ct);

    Task<Result<IEnumerable<TitleShortInfo>>> SearchTitlesAsync(string searchTerm, CancellationToken ct);
    
    Task<Result<IEnumerable<TitleShortInfo>>> FilterTitlesAsync(
        FilterRequest predicate, CancellationToken ct);
}
