using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;

namespace Persistence;

public static class ServiceExtensions
{
    public static void AddPersistenceServices(this IServiceCollection services)
    {
        var connectionString = Repositories.PostgreSQL.Configuration.GetConnectionString();
        Repositories.PostgreSQL.Configuration.Configure(connectionString);
        services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));
    }
}