using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extensions;

namespace Persistence.Repositories.PostgreSQL;

public static class Configuration
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
        {
            EnvironmentVariable<string>.PrintMissingEnvironmentVariables(errors);
            Environment.Exit(1);
        }

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
            Console.WriteLine("checking if the database is available");
            isAvailable = dbContext.Database.CanConnect();
            if (isAvailable)
                Console.WriteLine("database is available");
        }
        catch
        {
            Console.WriteLine("error connecting to database");
            isAvailable = false;
        }

        if (!isAvailable)
            Console.WriteLine("database is not available");

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
                Console.WriteLine("initial database tables were created successfully");
            return true;
        }
        catch
        {
            Console.WriteLine("initial database table create failed");
            return false;
        }
    }

    public static bool IsDatabaseMigrated(DataContext dbContext)
    {
        var anyPendingMigrations = dbContext.Database.GetPendingMigrations().Any();
        if (!anyPendingMigrations)
        {
            Console.WriteLine("no migrations necessary");
            return true;
        }

        var willMigrateDatabase = EnvironmentVariable<bool>.GetEnvironmentVariable(_migrateDatabase);
        if (!willMigrateDatabase)
        {
            Console.WriteLine($"there are pending database migrations needed, please set the environment variable {_migrateDatabase.Name} to true if you want to run the migrations");
            return false;
        }

        try
        {
            Console.WriteLine($"{_migrateDatabase.Name} is 'true', performing database migrations");
            dbContext.Database.Migrate();
            Console.WriteLine("database migrations successfully completed");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"error performing database migrations. Exception: {e.Message}");
            return false;
        }
    }
}