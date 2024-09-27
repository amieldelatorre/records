using Application.Features.UserFeatures.CreateUser;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceExtensions
{
    public static void AddUserFeatures(this IServiceCollection services)
    {
        services.AddScoped<CreateUserHandler>();
    }
}