using Persistence.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace WebAPI.Extensions;

public static class BuilderExtensions
{
    private static EnvironmentVariable<string> _logLevel = new("LOG_LEVEL", false, "INFO");

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
        var logLevelValue = EnvironmentVariable<string>.GetEnvironmentVariable(_logLevel);

        Serilog.Events.LogEventLevel logLevel;
        if (!Enum.TryParse<LogEventLevel>(logLevelValue, true, out logLevel))
            logLevel = Serilog.Events.LogEventLevel.Information;

        return logLevel;
    }
}