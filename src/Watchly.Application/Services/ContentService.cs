using Watchly.Application.Interfaces;
using Watchly.Application.Models;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;

namespace Watchly.Application.Services;

public class ContentService : IContentService
{
    public Task<Result<Title>> GetTitleByIdAsync(int titleId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<TitleShortInfo>>> SearchTitlesAsync(string searchTerm, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<TitleShortInfo>>> FilterTitlesAsync(FilterRequest predicate, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}