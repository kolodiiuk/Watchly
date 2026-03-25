using Watchly.Domain.Utils;

namespace Watchly.Infrastructure.Interfaces;

public interface IImageService
{
    Task<Result<string>> SaveImageAsync(Stream fileStream, string resourceName, CancellationToken ct);
}
