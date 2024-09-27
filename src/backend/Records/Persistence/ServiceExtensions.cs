using Application.Repositories.Database;
using Application.Repositories.DatabaseCache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
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

        services.AddScoped<IDatabaseCacheRepository, ValkeyDatabaseCacheRepository>();
        services.AddScoped<IUserRepository, PostgresUserRepository>();
        services.AddScoped<IUserDatabaseCacheRepository, UserDatabaseCacheRepository>();
    }
}