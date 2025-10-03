using System.Diagnostics;
using System.Text;
using Application.Repositories.Database;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Repositories.Database;
using Testcontainers.PostgreSql;
using TestsCommon;

namespace IntegrationTests;

public class PersistenceInfra
{
    // Required
    public required INetwork Network;
    public required Serilog.ILogger Logger { get; init; }
    // Database
    public PostgreSqlContainer? AppPostgresContainer { get; set; }
    public string? AppPostgresContainerUsername { get; set; } 
    // Repositories
    public IUserRepository? UserRepository { get; set; }
    public IWeightEntryRepository? WeightEntryRepository { get; set; }

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
         var weightEntryRepository = new PostgreSqlWeightEntryRepository(dbContext);
         WeightEntryRepository = weightEntryRepository;
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
        _persistenceInfra.AppPostgresContainerUsername = username;

        return this;
    }

    private async Task ConfigurePostgresDatabase(string sourcePath)
    {
        Debug.Assert(_persistenceInfra.AppPostgresContainer != null);

        await _persistenceInfra.AppPostgresContainer.StartAsync();
        var appPostgresConnectionString = _persistenceInfra.AppPostgresContainer.GetConnectionString();
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<DataContext>().UseNpgsql(appPostgresConnectionString);
        var dbContext = new DataContext(dbContextOptionsBuilder.Options);

        var cancellationTokenSource =  new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
        var dumpScriptString = await File.ReadAllTextAsync(sourcePath, cancellationTokenSource.Token);
        byte[] dumpScriptBytes = Encoding.UTF8.GetBytes(dumpScriptString);
        
        const string destPath = "/tmp/dump.sql";
        await _persistenceInfra.AppPostgresContainer.CopyAsync(
            dumpScriptBytes, 
            destPath,
            UnixFileModes.UserRead |  UnixFileModes.GroupRead | UnixFileModes.OtherRead,
            cancellationTokenSource.Token);
        
        var execResult = await _persistenceInfra.AppPostgresContainer.ExecAsync(
            [
                "psql", 
                "-U", 
                _persistenceInfra.AppPostgresContainerUsername,
                "-f",
                destPath,
            ], cancellationTokenSource.Token);
        if  (execResult.ExitCode != 0)
            Debug.Fail("Restoring database failure");

        var userRepository = new PostgreSqlUserRepository(dbContext);
        _persistenceInfra.UserRepository = userRepository;
        var weightEntryRepository = new PostgreSqlWeightEntryRepository(dbContext);
        _persistenceInfra.WeightEntryRepository = weightEntryRepository;
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