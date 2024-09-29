using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extensions;
using Serilog;

namespace Persistence.Repositories.Database;

public static class PostgresConfiguration
{
    private static EnvironmentVariable<string> _host = new("POSTGRES_HOST", true);
    private static EnvironmentVariable<string> _port = new("POSTGRES_PORT", true);
    private static EnvironmentVariable<string> _database = new("POSTGRES_DB", true);
    private static EnvironmentVariable<string> _username = new("POSTGRES_USER", true);
    private static EnvironmentVariable<string> _password = new("POSTGRES_PASSWORD", true);
    private static EnvironmentVariable<bool> _migrateDatabase = new("MIGRATE_DATABASE", false, false);

    public static string GetConnectionString()
    {
        var errors = new List<string>();
        var host = EnvironmentVariable<string>.GetEnvironmentVariable(_host);
        if (string.IsNullOrWhiteSpace(host) && _host.IsRequired)
            errors.Add(_host.Name);
        var port = EnvironmentVariable<string>.GetEnvironmentVariable(_port);
        if (string.IsNullOrWhiteSpace(port) && _host.IsRequired)
            errors.Add(_port.Name);
        var database = EnvironmentVariable<string>.GetEnvironmentVariable(_database);
        if (string.IsNullOrWhiteSpace(database) && _host.IsRequired)
            errors.Add(_database.Name);
        var username = EnvironmentVariable<string>.GetEnvironmentVariable(_username);
        if (string.IsNullOrWhiteSpace(username) && _host.IsRequired)
            errors.Add(_username.Name);
        var password = EnvironmentVariable<string>.GetEnvironmentVariable(_password);
        if (string.IsNullOrWhiteSpace(password) && _host.IsRequired)
            errors.Add(_password.Name);

        if (errors.Count > 0)
            EnvironmentVariable<string>.PrintMissingEnvironmentVariablesAndExit(errors);

        Debug.Assert(host != null && port != null && database != null && username != null && password != null);
        var connectionString = $"Host={host}; Port={port}; Database={database}; Username={username}; Password={password}";
        return connectionString;
    }

    private static DataContext GetDatabaseContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseNpgsql(connectionString);
        var dbContext = new DataContext(optionsBuilder.Options);
        return dbContext;
    }

    public static void Configure(string connectionString)
    {
        var dbContext = GetDatabaseContext(connectionString);
        if (!(IsAvailable(dbContext) && IsDatabaseMigrated(dbContext)))
        {
            dbContext.Database.CloseConnection();
            Environment.Exit(1);
        }
        dbContext.Database.CloseConnection();
    }

    public static bool IsAvailable(DataContext dbContext)
    {
        bool isAvailable;
        try
        {
            Log.Logger.Information("checking if the database is available");
            isAvailable = dbContext.Database.CanConnect();
            if (isAvailable)
                Log.Logger.Information("database is available");
        }
        catch(Exception ex)
        {
            Log.Logger.Error("error connecting to database. {error}", ex.Message);
            isAvailable = false;
        }

        if (!isAvailable)
            Log.Logger.Error("database is not available");

        return isAvailable;
    }

    // The EnsureCreated method does not add to the migrations table
    //      https://learn.microsoft.com/en-us/aspnet/core/data/ef-rp/migrations?view=aspnetcore-8.0&tabs=visual-studio#remove-ensurecreated
    // This causes issues if it is called before migrating the database. Therefore, it is not used here.
    // Keeping the code and comment here as a reminder.
    public static bool IsDatabaseCreated(DataContext dbContext)
    {
        try
        {
            var databaseCreated = dbContext.Database.EnsureCreated();
            if (databaseCreated)
                Log.Logger.Information("initial database tables were created successfully");
            return true;
        }
        catch (Exception ex)
        {
            Log.Logger.Error("initial database table create failed. {error}", ex.Message);
            return false;
        }
    }

    public static bool IsDatabaseMigrated(DataContext dbContext)
    {
        var anyPendingMigrations = dbContext.Database.GetPendingMigrations().Any();
        if (!anyPendingMigrations)
        {
            Log.Logger.Information("no migrations necessary");
            return true;
        }

        var willMigrateDatabase = EnvironmentVariable<bool>.GetEnvironmentVariable(_migrateDatabase);
        if (!willMigrateDatabase)
        {
            Log.Logger.Error("there are pending database migrations needed, please set the environment variable {envVarName} to true if you want to run the migrations", _migrateDatabase.Name);
            return false;
        }

        try
        {
            Log.Logger.Warning("{envVarName} is 'true', performing database migrations", _migrateDatabase.Name);
            dbContext.Database.Migrate();
            Log.Logger.Warning("database migrations successfully completed");
            return true;
        }
        catch (Exception ex)
        {
            Log.Logger.Error("error performing database migrations. {error}", ex.Message);
            return false;
        }
    }
}