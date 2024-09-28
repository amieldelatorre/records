using Application.Features.UserFeatures.CreateUser;
using Application.Repositories.DatabaseCache;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceExtensions
{
    public static void AddUserFeatures(this IServiceCollection services)
    {
        services.AddScoped<ICachedUserRepository, CachedUserRepository>();
        services.AddScoped<CreateUserHandler>();
    }
}