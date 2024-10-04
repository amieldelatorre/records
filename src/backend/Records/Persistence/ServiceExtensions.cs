using Application.Configuration.EnvironmentVariables;
using Application.Repositories.Database;
using Application.Repositories.DatabaseCache;
using Application.Repositories.FeatureToggle;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
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

        CacheConfiguration.EnableCaching.GetEnvironmentVariable();
        if (CacheConfiguration.EnableCaching.Value)
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

        FeatureToggleConfiguration.EnableFeatureToggles.GetEnvironmentVariable();
        if (FeatureToggleConfiguration.EnableFeatureToggles.Value)
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