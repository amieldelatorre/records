using System.Diagnostics;
using Application.Configuration.EnvironmentVariables;
using Serilog;
using StackExchange.Redis;

namespace Persistence.Repositories.DatabaseCache;

public class ValkeyConfiguration
{
    private static StringEnvironmentVariable _host = new("RECORDS__VALKEY_HOST", true);
    private static StringEnvironmentVariable _port = new("RECORDS__VALKEY_PORT", true);

    public static string GetConnectionString()
    {
        Log.Logger.Information("environment variable '{envVarName}' is set to 'true'. Retrieving Valkey connection details", CacheConfiguration.EnableCaching.Name);
        var errors = new List<EnvironmentVariable>();

        _host.GetEnvironmentVariable();
        if (!_host.IsValid)
            errors.Add(_host);

        _port.GetEnvironmentVariable();
        if (!_port.IsValid)
            errors.Add(_port);

        if (errors.Count > 0)
            EnvironmentVariable.PrintMissingEnvironmentVariablesAndExit(errors);

        var connectionString = $"{_host.Value}:{_port.Value}";
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