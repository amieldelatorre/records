using Application.Repositories.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Persistence.Repositories.Database;
using Serilog;

namespace Persistence;

public static class ServiceExtensions
{
    public static void AddPersistenceServices(this IServiceCollection services)
    {
        Log.Logger.Information("Adding Persistence Services");
        var postgreSqlConnectionString = PostgreSqlConfiguration.GetConnectionString();
        PostgreSqlConfiguration.Configure(postgreSqlConnectionString);
        services.AddDbContext<DataContext>(options => options.UseNpgsql(postgreSqlConnectionString));
        
        services.AddScoped<IUserRepository, PostgreSqlUserRepository>();
    }
}