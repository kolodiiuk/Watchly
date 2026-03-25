using Watchly.Domain.Entities;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IContentService
{
    Task<Result<Title>> GetTitleByIdAsync(CancellationToken ct);

    Task<Result<IEnumerable<Title>>> GetTitlesByConditionAsync(Func<Title, bool> p, CancellationToken ct);
}
