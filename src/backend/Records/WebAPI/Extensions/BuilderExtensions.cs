using Application.Configuration.EnvironmentVariables;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace WebAPI.Extensions;

public static class BuilderExtensions
{
    private static readonly StringEnvironmentVariable LogLevel = new("LOG_LEVEL", false, "INFO");

    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        var logLevel = GetLogLevel();

        var logConfiguration = new LoggerConfiguration()
            .WriteTo.Console(new JsonFormatter())
            .MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(logLevel))
            .MinimumLevel.Override("Microsoft",LogEventLevel.Warning)
            .MinimumLevel.Override("Unleash", LogEventLevel.Warning)
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName();

        Serilog.Log.Logger = logConfiguration.CreateLogger();
        builder.Services.AddSingleton(Serilog.Log.Logger);
        builder.Host.UseSerilog();
    }

    private static LogEventLevel GetLogLevel()
    {
        LogLevel.GetEnvironmentVariable();
        if (!Enum.TryParse<LogEventLevel>(BuilderExtensions.LogLevel.Value, true, out var level))
            level = LogEventLevel.Information;

        return level;
    }

    
}