using Application.Configuration.EnvironmentVariables;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace WebAPI.Extensions;

public static class BuilderExtensions
{
    private static StringEnvironmentVariable _logLevel = new("LOG_LEVEL", false, "INFO");

    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        var logLevel = GetLogLevel();

        var logConfiguration = new LoggerConfiguration()
            .WriteTo.Console(new JsonFormatter())
            .MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(logLevel))
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Unleash", Serilog.Events.LogEventLevel.Warning)
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName();

        Serilog.Log.Logger = logConfiguration.CreateLogger();
        builder.Services.AddSingleton(Serilog.Log.Logger);
        builder.Host.UseSerilog();
    }

    private static Serilog.Events.LogEventLevel GetLogLevel()
    {
        _logLevel.GetEnvironmentVariable();
        Serilog.Events.LogEventLevel logLevel;
        if (!Enum.TryParse<LogEventLevel>(_logLevel.Value, true, out logLevel))
            logLevel = Serilog.Events.LogEventLevel.Information;

        return logLevel;
    }
}