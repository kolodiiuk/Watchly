using CloudinaryDotNet;

namespace Watchly.Api.Extensions;

public static class IServiceCollectionExtensions
{
    public static void RegisterCloudinary(
        this IServiceCollection services, string cloud, string apiKey, string apiSecret)
        => services.AddSingleton(new Cloudinary(new Account(cloud, apiKey, apiSecret)));
}
