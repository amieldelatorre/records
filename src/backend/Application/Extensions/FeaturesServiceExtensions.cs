using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Application.Features.AuthFeatures.Login;
using Application.Features.UserFeatures.CreateUser;
using Application.Features.UserFeatures.DeleteUser;
using Application.Features.UserFeatures.GetUser;
using Application.Features.UserFeatures.UpdateUser;
using Application.Features.UserFeatures.UpdateUserPassword;
using Application.Features.WeightEntryFeatures.CreateWeightEntry;
using Application.Features.WeightEntryFeatures.DeleteWeightEntry;
using Application.Features.WeightEntryFeatures.GetWeightEntry;
using Application.Features.WeightEntryFeatures.ListWeightEntry;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class FeaturesServiceExtensions
{
    public static void AddFeatureServices(this IServiceCollection services)
    {
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<GetUserHandler>();
        services.AddScoped<UpdateUserHandler>();
        services.AddScoped<UpdateUserPasswordHandler>();
        services.AddScoped<DeleteUserHandler>();
        
        services.AddScoped<LoginHandler>();
        services.AddScoped<JwtCreateHandler>();

        services.AddScoped<CreateWeightEntryHandler>();
        services.AddScoped<GetWeightEntryHandler>();
        services.AddScoped<ListWeightEntryHandler>();
        services.AddScoped<DeleteWeightEntryHandler>();
    }
}