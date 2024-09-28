using Application.Repositories.Database;
using Application.Repositories.DatabaseCache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.Extensions;
using Persistence.Repositories.Database;
using Persistence.Repositories.DatabaseCache;

namespace Persistence;

public static class ServiceExtensions
{
    public static void AddPersistenceServices(this IServiceCollection services)
    {
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
    }
}