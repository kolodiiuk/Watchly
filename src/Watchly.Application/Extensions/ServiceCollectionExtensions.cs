using Microsoft.Extensions.DependencyInjection;
using Watchly.Application.Interfaces;
using Watchly.Application.Services;

namespace Watchly.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAuthService, AuthService>();
        serviceCollection.AddSingleton<IJwtService, JwtService>();
        serviceCollection.AddScoped<IUserManagementService, UserManagementService>();
        serviceCollection.AddScoped<IPasswordManagementService, PasswordManagementService>();
        serviceCollection.AddScoped<ICommentService, CommentService>();
        serviceCollection.AddScoped<IContentService, ContentService>();
    }
}
