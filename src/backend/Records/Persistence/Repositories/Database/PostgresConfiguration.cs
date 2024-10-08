using System.Diagnostics;
using Application.Configuration.EnvironmentVariables;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Serilog;

namespace Persistence.Repositories.Database;

public static class PostgresConfiguration
{
    private static StringEnvironmentVariable _host = new("RECORDS__POSTGRES_HOST", true);
    private static StringEnvironmentVariable _port = new("RECORDS__POSTGRES_PORT", true);
    private static StringEnvironmentVariable _database = new("RECORDS__POSTGRES_DB", true);
    private static StringEnvironmentVariable _username = new("RECORDS__POSTGRES_USER", true);
    private static StringEnvironmentVariable _password = new("RECORDS__POSTGRES_PASSWORD", true);
    private static BoolEnvironmentVariable _migrateDatabase = new("RECORDS__MIGRATE_DATABASE", false, false);

    public static string GetConnectionString()
    {
        var errors = new List<EnvironmentVariable>();
        _host.GetEnvironmentVariable();
        if (!_host.IsValid)
            errors.Add(_host);
        _port.GetEnvironmentVariable();
        if (!_port.IsValid)
            errors.Add(_port);
        _database.GetEnvironmentVariable();
        if (!_database.IsValid)
            errors.Add(_database);
        _username.GetEnvironmentVariable();
        if (!_username.IsValid)
            errors.Add(_username);
        _password.GetEnvironmentVariable();
        if (!_password.IsValid)
            errors.Add(_password);
        _migrateDatabase.GetEnvironmentVariable();
        if (!_migrateDatabase.IsValid)
            errors.Add(_migrateDatabase);

        if (errors.Count > 0)
            EnvironmentVariable.PrintMissingEnvironmentVariablesAndExit(errors);

        var connectionString = $"Host={_host.Value}; Port={_port.Value}; Database={_database.Value}; Username={_username.Value}; Password={_password.Value}";
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

        if (!_migrateDatabase.Value)
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