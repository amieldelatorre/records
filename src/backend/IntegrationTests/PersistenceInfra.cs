using System.Diagnostics;
using Application.Repositories.Database;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence.Contexts;
using Persistence.Repositories.Database;
using Testcontainers.PostgreSql;
using TestsCommon;

namespace IntegrationTests;

public class PersistenceInfra
{
    // Required
    public required INetwork Network;
    public required Serilog.ILogger Logger { get; set; }
    // Database
    public  PostgreSqlContainer? AppPostgresContainer { get; set; }
    // Repositories
    public IUserRepository? UserRepository { get; set; }

    public async Task Dispose()
    {
        if (AppPostgresContainer != null)
            await AppPostgresContainer.StopAsync();
        await Network.DisposeAsync();
    }    
    public void RenewDbContext()
     {
         Debug.Assert(AppPostgresContainer != null);
         var appPostgresConnectionString = AppPostgresContainer.GetConnectionString();
         var dbContextOptionsBuilder = new DbContextOptionsBuilder<DataContext>().UseNpgsql(appPostgresConnectionString);
         var dbContext = new DataContext(dbContextOptionsBuilder.Options);
         
         var userRepository = new PostgreSqlUserRepository(dbContext);
         UserRepository = userRepository;
     }
}

public class PersistenceInfraBuilder
{
    private readonly PersistenceInfra _persistenceInfra;

    public PersistenceInfraBuilder()
    {
        var logger = TestLogger.GetLogger();
        var network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();

        _persistenceInfra = new PersistenceInfra
        {
            Network = network,
            Logger = logger,
        };
    }

    public PersistenceInfraBuilder AddPostgresDatabase()
    {
        const string databaseName = "records";
        const string username = "records";
        const string password = "password";
        var container = GetPostgreSqlContainer(_persistenceInfra.Network, databaseName, username, password);

        _persistenceInfra.AppPostgresContainer = container;

        return this;
    }

    private async Task ConfigurePostgresDatabase(string filepath)
    {
        Debug.Assert(_persistenceInfra.AppPostgresContainer != null);

        await _persistenceInfra.AppPostgresContainer.StartAsync();
        var appPostgresConnectionString = _persistenceInfra.AppPostgresContainer.GetConnectionString();
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<DataContext>().UseNpgsql(appPostgresConnectionString);
        var dbContext = new DataContext(dbContextOptionsBuilder.Options);

        NpgsqlConnection dbConnection = new(appPostgresConnectionString);
        await dbConnection.OpenAsync();
        var sqlScript = await File.ReadAllTextAsync(filepath);
        var pgCommand = new NpgsqlCommand(sqlScript, dbConnection);
        await pgCommand.ExecuteNonQueryAsync();
        await dbConnection.CloseAsync();

        var userRepository = new PostgreSqlUserRepository(dbContext);
        _persistenceInfra.UserRepository = userRepository;
    }

    public async Task<PersistenceInfra> Build()
    {
        if (_persistenceInfra.AppPostgresContainer != null)
            await ConfigurePostgresDatabase(Path.Combine(Directory.GetCurrentDirectory(), "Data", "app_postgres_dump.sql"));

        return _persistenceInfra;
    }

    private static PostgreSqlContainer GetPostgreSqlContainer(INetwork containerNetwork, string databaseName,
        string username, string password)
    {
        var container = new PostgreSqlBuilder()
            .WithImage("postgres:18.0")
            .WithDatabase(databaseName)
            .WithUsername(username)
            .WithPassword(password)
            .WithNetwork(containerNetwork)
            .Build();

        return container;
    }
}