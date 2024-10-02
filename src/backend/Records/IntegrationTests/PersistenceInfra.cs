using System.Diagnostics;
using Application.Features;
using Application.Repositories.Database;
using Application.Repositories.DatabaseCache;
using Application.Repositories.FeatureToggle;
using Common;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence.Context;
using Persistence.Repositories.Database;
using Persistence.Repositories.DatabaseCache;
using Persistence.Repositories.FeatureToggle;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Unleash;
using Unleash.ClientFactory;

namespace IntegrationTests;

public class PersistenceInfra
{
    // Required
    public required INetwork Network;
    public required Serilog.ILogger Logger { get; set; }

    // Database
    public  PostgreSqlContainer? RecordsPostgresContainer { get; set; }

    // Caching
    public IContainer? ValkeyContainer { get; set; }

    // Feature Toggle
    public PostgreSqlContainer? UnleashPostgresContainer { get; set; }
    public IContainer? UnleashWebContainer { get; set; }
    public FeatureStatus? RecordsFeatureStatus { get; set; }

    // Repositories
    public ICacheRepository? CacheRepository { get; set; }
    public ICachedUserRepository? CachedUserRepository { get; set; }
    public IFeatureToggleRepository? FeatureToggleRepository { get; set; }
    public IUserRepository? UserRepository { get; set; }

    public async Task Dispose()
    {
        if (RecordsPostgresContainer != null)
            await RecordsPostgresContainer.StopAsync();
        if (UnleashPostgresContainer != null)
            await UnleashPostgresContainer.StopAsync();
        if (UnleashWebContainer != null)
            await UnleashWebContainer.StopAsync();
        if (ValkeyContainer != null)
            await ValkeyContainer.StopAsync();
        await Network.DisposeAsync();
    }
}

public class PersistenceInfraBuilder
{
    private readonly PersistenceInfra _persistenceInfra;
    private ushort _valkeyContainerPort = 6379;
    private string _unleashPostgresDatabaseName = "unleash";
    private string _unleashPostgresUsername = "root";
    private string _unleashPostgresPassword = "password";
    private ushort _unleashContainerPort = 4242;

    public PersistenceInfraBuilder()
    {
        var logger = TestLogger.GetLogger();
        var network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();

        var nullCacheRepository = new NullCacheRepository();
        var nullFeatureToggleRepository = new NullFeatureToggleRepository();

        _persistenceInfra = new PersistenceInfra
        {
            Network = network,
            Logger = logger,
            CacheRepository = nullCacheRepository,
            FeatureToggleRepository = nullFeatureToggleRepository,
        };
    }

    public PersistenceInfraBuilder AddPostgresDatabase()
    {
        const string databaseName = "records";
        const string username = "root";
        const string password = "password";
        var container = GetPostgreSqlContainer(_persistenceInfra.Network, databaseName, username, password);

        _persistenceInfra.RecordsPostgresContainer = container;

        return this;
    }

    private async Task ConfigurePostgresDatabase(string filepath)
    {
        Debug.Assert(_persistenceInfra.RecordsPostgresContainer != null);

        await _persistenceInfra.RecordsPostgresContainer.StartAsync();
        var recordsPostgresConnectionString = _persistenceInfra.RecordsPostgresContainer.GetConnectionString();
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<DataContext>().UseNpgsql(recordsPostgresConnectionString);
        var dbContext = new DataContext(dbContextOptionsBuilder.Options);

        NpgsqlConnection dbConnection = new(recordsPostgresConnectionString);
        await dbConnection.OpenAsync();
        var sqlScript = await File.ReadAllTextAsync(filepath);
        var pgCommand = new NpgsqlCommand(sqlScript, dbConnection);
        await pgCommand.ExecuteNonQueryAsync();
        await dbConnection.CloseAsync();

        var userRepository = new PostgresUserRepository(dbContext);
        _persistenceInfra.UserRepository = userRepository;
    }

    public PersistenceInfraBuilder AddUnleashFeatureToggles()
    {
        var unleashPostgresContainer = GetPostgreSqlContainer(_persistenceInfra.Network, _unleashPostgresDatabaseName,
            _unleashPostgresUsername, _unleashPostgresPassword);

        _persistenceInfra.UnleashPostgresContainer = unleashPostgresContainer;

        return this;
    }

    private async Task ConfigureUnleashFeatureToggles(string filepath)
    {
        Debug.Assert(_persistenceInfra.UnleashPostgresContainer != null && _unleashPostgresUsername != null &&
                     _unleashPostgresPassword != null && _unleashPostgresDatabaseName != null);

        await _persistenceInfra.UnleashPostgresContainer.StartAsync();
        NpgsqlConnection dbConnection = new(_persistenceInfra.UnleashPostgresContainer.GetConnectionString());
        await dbConnection.OpenAsync();
        var sqlScript = await File.ReadAllTextAsync(filepath);
        var pgCommand = new NpgsqlCommand(sqlScript, dbConnection);
        await pgCommand.ExecuteNonQueryAsync();
        await dbConnection.CloseAsync();

        var dbHost = _persistenceInfra.UnleashPostgresContainer.Name.TrimStart('/');
        var unleashWebContainer = await GetUnleashWebContainer(_persistenceInfra.Network, _unleashContainerPort, dbHost,
            _unleashPostgresDatabaseName, _unleashPostgresUsername, _unleashPostgresPassword);

        var unleashSettings = new UnleashSettings
        {
            AppName = "records",
            UnleashApi =  new UriBuilder(Uri.UriSchemeHttp, unleashWebContainer.Hostname,
                unleashWebContainer.GetMappedPublicPort(_unleashContainerPort), "api").Uri,
            CustomHttpHeaders = new Dictionary<string, string>
            {
                { "Authorization", "default:production.98d90dccb96f15c73194e8248df6d839ffbc26c0ed5a8459943f1a97"}
            }
        };
        var unleashFactory = new UnleashClientFactory();
        var unleashClient = await unleashFactory.CreateClientAsync(unleashSettings, synchronousInitialization: true);
        var featureToggleRepository = new UnleashFeatureToggleRepository(unleashClient, _persistenceInfra.Logger);

        _persistenceInfra.UnleashWebContainer = unleashWebContainer;
        _persistenceInfra.FeatureToggleRepository = featureToggleRepository;
    }

    public PersistenceInfraBuilder AddValkeyCaching()
    {
        var container = new ContainerBuilder()
            .WithImage("valkey/valkey:8.0")
            .WithPortBinding(_valkeyContainerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(_valkeyContainerPort))
            .WithNetwork(_persistenceInfra.Network)
            .Build();

        _persistenceInfra.ValkeyContainer = container;
        return this;
    }

    private async Task ConfigureValkeyCaching()
    {
        Debug.Assert(_persistenceInfra.ValkeyContainer != null && _persistenceInfra.UserRepository != null);
        await _persistenceInfra.ValkeyContainer.StartAsync();

        var connectionString = $"{_persistenceInfra.ValkeyContainer.Hostname}:{_persistenceInfra.ValkeyContainer.GetMappedPublicPort(_valkeyContainerPort)}";
        var valkeyConnectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);
        var valkeyDatabase = valkeyConnectionMultiplexer.GetDatabase();

        var cacheRepository = new ValkeyCacheRepository(valkeyDatabase, _persistenceInfra.Logger);
        var cachedUserRepository = new CachedUserRepository(cacheRepository, _persistenceInfra.UserRepository, _persistenceInfra.Logger);

        _persistenceInfra.CacheRepository = cacheRepository;
        _persistenceInfra.CachedUserRepository = cachedUserRepository;
    }

    public async Task<PersistenceInfra> Build()
    {
        if (_persistenceInfra.RecordsPostgresContainer != null)
            await ConfigurePostgresDatabase(Path.Combine(Directory.GetCurrentDirectory(), "Data", "records_postgres_dump.sql"));
        if (_persistenceInfra.UnleashPostgresContainer != null)
            await ConfigureUnleashFeatureToggles(Path.Combine(Directory.GetCurrentDirectory(), "Data", "unleash_postgres_dump.sql"));
        if (_persistenceInfra.ValkeyContainer != null)
            await ConfigureValkeyCaching();

        Debug.Assert(_persistenceInfra.FeatureToggleRepository != null && _persistenceInfra.CacheRepository != null &&
                     _persistenceInfra.UserRepository != null);
        var featureStatus = new FeatureStatus(_persistenceInfra.FeatureToggleRepository, _persistenceInfra.CacheRepository, _persistenceInfra.Logger);
        var cachedUserRepository = new CachedUserRepository(_persistenceInfra.CacheRepository, _persistenceInfra.UserRepository, _persistenceInfra.Logger);

        _persistenceInfra.RecordsFeatureStatus = featureStatus;
        _persistenceInfra.CachedUserRepository = cachedUserRepository;
        return _persistenceInfra;
    }

    private static PostgreSqlContainer GetPostgreSqlContainer(INetwork containerNetwork, string databaseName,
        string username, string password)
    {
        var container = new PostgreSqlBuilder()
            .WithImage("postgres:17.0")
            .WithDatabase(databaseName)
            .WithUsername(username)
            .WithPassword(password)
            .WithNetwork(containerNetwork)
            .Build();

        return container;
    }

    private static async Task<IContainer> GetUnleashWebContainer(INetwork containerNetwork, ushort containerPort,
        string dbHost, string dbName, string dbUsername, string dbPassword)
    {
        var databaseUrl = $"postgres://{dbUsername}:{dbPassword}@{dbHost}:5432/{dbName}";

        var container = new ContainerBuilder()
            .WithImage("unleashorg/unleash-server:6.2")
            .WithPortBinding(containerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(containerPort)))
            .WithEnvironment("DATABASE_URL", databaseUrl)
            .WithEnvironment("DATABASE_SSL", "false")
            .WithNetwork(containerNetwork)
            .Build();

        await container.StartAsync();
        return container;
    }
}