using Application.Configurations.EnvironmentVariables;
using Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Persistence.Repositories.Database;

public class PostgreSqlConfiguration
{
    private static readonly StringEnvironmentVariable Host = new("APP__POSTGRES_HOST", true);
    private static readonly StringEnvironmentVariable Port = new("APP__POSTGRES_PORT", true);
    private static readonly StringEnvironmentVariable Database = new("APP__POSTGRES_DB", true);
    private static readonly StringEnvironmentVariable Username = new("APP__POSTGRES_USER", true);
    private static readonly StringEnvironmentVariable Password = new("APP__POSTGRES_PASSWORD", true);
    private static readonly BoolEnvironmentVariable MigrateDatabase = new("APP__MIGRATE_DATABASE", false, false);

    public static string GetConnectionString()
    {
        var errors = new List<AbstractEnvironmentVariable>();
        Host.GetEnvironmentVariable();
        if (!Host.IsValid)
            errors.Add(Host);
        Port.GetEnvironmentVariable();
        if (!Port.IsValid)
            errors.Add(Port);
        Database.GetEnvironmentVariable();
        if (!Database.IsValid)
            errors.Add(Database);
        Username.GetEnvironmentVariable();
        if (!Username.IsValid)
            errors.Add(Username);
        Password.GetEnvironmentVariable();
        if (!Password.IsValid)
            errors.Add(Password);
        MigrateDatabase.GetEnvironmentVariable();
        if (!MigrateDatabase.IsValid)
            errors.Add(MigrateDatabase);

        if (errors.Count > 0)
            AbstractEnvironmentVariable.PrintMissingEnvironmentVariablesAndExit(errors);

        var connectionString = $"Host={Host.Value}; Port={Port.Value}; Database={Database.Value}; Username={Username.Value}; Password={Password.Value}";
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

    private static bool IsAvailable(DataContext dbContext)
    {
        bool isAvailable;
        try
        {
            Log.Logger.Information("checking if the database is available");
            isAvailable = dbContext.Database.CanConnect();
            if (isAvailable)
                Log.Logger.Information("database is available");
        }
        catch (Exception ex)
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

    private static bool IsDatabaseMigrated(DataContext dbContext)
    {
        var anyPendingMigrations = dbContext.Database.GetPendingMigrations().Any();
        if (!anyPendingMigrations)
        {
            Log.Logger.Information("no migrations necessary");
            return true;
        }

        if (!MigrateDatabase.Value)
        {
            Log.Logger.Error("there are pending database migrations needed, please set the environment variable {envVarName} to true if you want to run the migrations", MigrateDatabase.Name);
            return false;
        }

        try
        {
            Log.Logger.Warning("{envVarName} is 'true', performing database migrations", MigrateDatabase.Name);
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