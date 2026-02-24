using Microsoft.Extensions.DependencyInjection;
using Watchly.Infrastructure.Interfaces;

namespace Watchly.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRepositories(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IRefreshTokenRepository, RefreshTokenRepoStub>();
    }
}
