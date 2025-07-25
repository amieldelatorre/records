using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Application.Features.AuthFeatures.Login;
using Application.Features.UserFeatures.CreateUser;
using Application.Features.UserFeatures.GetUser;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class FeaturesServiceExtensions
{
    public static void AddFeatureServices(this IServiceCollection services)
    {
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<GetUserHandler>();
        
        services.AddScoped<LoginHandler>();
        services.AddScoped<JwtCreateHandler>();
    }
}