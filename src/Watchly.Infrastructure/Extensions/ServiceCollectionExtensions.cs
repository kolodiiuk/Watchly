using Microsoft.Extensions.DependencyInjection;
using Watchly.Infrastructure.Interfaces;
using Watchly.Infrastructure.Repositories;
using Watchly.Infrastructure.Services;

namespace Watchly.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRepositories(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        serviceCollection.AddScoped<IEmailService, EmailService>();
    }
}
