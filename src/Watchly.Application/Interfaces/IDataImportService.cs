using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IDataImportService
{
    Task<Result> ImportTmdbDataAsync(int titleId);

}
