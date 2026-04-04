using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.Interfaces;

namespace Watchly.Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public ImageService(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<Result<string>> SaveImageAsync(
        Stream fileStream, string resourceName, CancellationToken ct)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(resourceName, fileStream),
            PublicId = Guid.NewGuid().ToString(),
            Overwrite = true,
            Folder = "uploads/"
        };
        try
        {
            var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);

            if (uploadResult.StatusCode != HttpStatusCode.OK)
            {
                return Result<string>.Fail(
                    $"Saving image failed: {uploadResult.StatusCode}");
            }

            return Result<string>.Success(uploadResult.Url.AbsoluteUri);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            return Result<string>.Fail(
                $"{e.Message}{Environment.NewLine}{e.InnerException?.Message ?? ""}");
        }
    }
}
