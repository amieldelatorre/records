using Application.Repositories.Database;
using Application.Repositories.DatabaseCache;
using Application.Repositories.FeatureToggle;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.Extensions;
using Persistence.Repositories.Database;
using Persistence.Repositories.DatabaseCache;
using Persistence.Repositories.FeatureToggle;
using Serilog;

namespace Persistence;

public static class ServiceExtensions
{
    public static void AddPersistenceServices(this IServiceCollection services)
    {
        Log.Logger.Information("registering services for data storage");
        var connectionString = PostgresConfiguration.GetConnectionString();
        PostgresConfiguration.Configure(connectionString);
        services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));

        var enableCaching = EnvironmentVariable<bool>.GetEnvironmentVariable(CacheConfiguration.EnableCaching);
        if (enableCaching)
        {
            var valkeyConnectionString = ValkeyConfiguration.GetConnectionString();
            var valkeyDatabase = ValkeyConfiguration.GetDatabase(valkeyConnectionString);

            services.AddSingleton(valkeyDatabase);
            services.AddScoped<ICacheRepository, ValkeyCacheRepository>();
        }
        else
        {
            services.AddScoped<ICacheRepository, NullCacheRepository>();
        }

        services.AddScoped<IUserRepository, PostgresUserRepository>();

        var enableFeatureToggles = EnvironmentVariable<bool>.GetEnvironmentVariable(FeatureToggleConfiguration.EnableFeatureToggles);
        if (enableFeatureToggles)
        {
            var unleashClient = UnleashConfiguration.GetClient();
            services.AddSingleton(unleashClient);
            services.AddScoped<IFeatureToggleRepository, UnleashFeatureToggleRepository>();
        }
        else
        {
            services.AddScoped<IFeatureToggleRepository, NullFeatureToggleRepository>();
        }
    }
}