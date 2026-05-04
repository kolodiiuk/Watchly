using CloudinaryDotNet;

namespace Watchly.Api.Extensions;

public static class IServiceCollectionExtensions
{
    public static void RegisterCloudinary(
        this IServiceCollection services, string cloud, string apiKey, string apiSecret)
        => services.AddSingleton(new Cloudinary(new Account(cloud, apiKey, apiSecret)));

    public static void RegisterOutputCache(this IServiceCollection services, string connectionString)
    {
        services.AddStackExchangeRedisOutputCache(o =>
        {
            o.Configuration = connectionString;
            o.InstanceName = "Watchly_DistributedCache_";
        });

        services.AddOutputCache(options =>
        {
            options.AddPolicy("TitleById",
                b => b.Expire(TimeSpan.FromMinutes(20))
                        .SetVaryByRouteValue(["titleId"]));
        });
    }
}
