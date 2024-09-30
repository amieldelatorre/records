using System.Diagnostics;
using Persistence.Extensions;
using Serilog;
using StackExchange.Redis;

namespace Persistence.Repositories.DatabaseCache;

public class ValkeyConfiguration
{
    private static EnvironmentVariable<string> _host = new("RECORDS__VALKEY_HOST", true);
    private static EnvironmentVariable<string> _port = new("RECORDS__VALKEY_PORT", true);

    public static string GetConnectionString()
    {
        Log.Logger.Information("environment variable '{envVarName}' is set to 'true'. Retrieving Valkey connection details", CacheConfiguration.EnableCaching.Name);
        var errors = new List<string>();

        var host = EnvironmentVariable<string>.GetEnvironmentVariable(_host);
        if (string.IsNullOrWhiteSpace(host))
            errors.Add(_host.Name);

        var port = EnvironmentVariable<string>.GetEnvironmentVariable(_port);
        if (string.IsNullOrWhiteSpace(port))
            errors.Add(_port.Name);

        if (errors.Count > 0)
            EnvironmentVariable<string>.PrintMissingEnvironmentVariablesAndExit(errors);

        Debug.Assert(host != null && port != null);
        var connectionString = $"{host}:{port}";
        return connectionString;
    }

    // <summary>
    // The ConnectionMultiplexer.Connect() method checks if the database is reachable already
    // </summary>
    public static IDatabase GetDatabase(string connectionString)
    {
        IDatabase database;
        try
        {
            var multiplexer = ConnectionMultiplexer.Connect(connectionString);
            database = multiplexer.GetDatabase();
        }
        catch (Exception ex)
        {
            Log.Logger.Error("an error occurred while connecting to the valkey database: {error}", ex.Message);
            Environment.Exit(1);
            database = null;
        }
        return database;
    }

    public static bool IsAvailable(IDatabase database)
    {
        try
        {
            var latency = database.Ping();
            Log.Logger.Information("valkey ping latency: {valkeyPingLatency}", latency);
            return true;
        }
        catch (Exception ex)
        {
            Log.Logger.Error("error pinging valkey database: {error}", ex.Message);
            return false;
        }
    }
}