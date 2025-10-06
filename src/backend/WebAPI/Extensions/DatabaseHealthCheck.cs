using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Persistence.Contexts;

namespace WebAPI.Extensions;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly DataContext _dbContext;
    
    public DatabaseHealthCheck(DataContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            if (canConnect)
                return HealthCheckResult.Healthy("Application is able to connect to the database.");
            return HealthCheckResult.Unhealthy("Application is unable to connect to the database.");
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Application is unable to connect to the database.");
        }
    }
}