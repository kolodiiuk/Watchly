using Watchly.Application.Models.AdminContent;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IAdminContentService
{
    Task<Result<int>> AddTitleAsync(CreateTitleRequest request, CancellationToken ct = default);

    Task<Result<TitleReferenceOptions>> GetTitleReferenceOptionsAsync(
        string? productionCompanyTerm,
        IReadOnlyCollection<int>? selectedProductionCompanyIds,
        CancellationToken ct = default);

    Task<Result> UpdateTitleAsync(int titleId, UpdateTitleRequest request, CancellationToken ct = default);

    Task<Result> UploadPosterAsync(int titleId, Stream posterStream, CancellationToken ct = default);

    Task<Result> SoftDeleteTitleAsync(int titleId, CancellationToken ct = default);

    Task<Result<int>> AddSeasonAsync(int titleId, CreateSeasonRequest request, CancellationToken ct = default);

    Task<Result> UpdateSeasonAsync(int seasonId, UpdateSeasonRequest request, CancellationToken ct = default);

    Task<Result> RemoveSeasonAsync(int seasonId, CancellationToken ct = default);

    Task<Result<int>> AddEpisodeAsync(int seasonId, CreateEpisodeRequest request, CancellationToken ct = default);

    Task<Result> UpdateEpisodeAsync(int episodeId, UpdateEpisodeRequest request, CancellationToken ct = default);

    Task<Result> RemoveEpisodeAsync(int episodeId, CancellationToken ct = default);
}
