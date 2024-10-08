using Application.Features;
using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Application.Features.AuthFeatures.Login;
using Application.Features.UserFeatures.CreateUser;
using Application.Features.UserFeatures.DeleteUser;
using Application.Features.UserFeatures.GetUser;
using Application.Features.UserFeatures.UpdateUser;
using Application.Features.UserFeatures.UpdateUserPassword;
using Application.Repositories.DatabaseCache;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<FeatureStatus>();

        services.AddScoped<ICachedUserRepository, CachedUserRepository>();
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<GetUserHandler>();
        services.AddScoped<UpdateUserHandler>();
        services.AddScoped<UpdateUserPasswordHandler>();
        services.AddScoped<DeleteUserHandler>();

        services.AddScoped<LoginHandler>();
        services.AddScoped<JwtCreateHandler>();
    }
}