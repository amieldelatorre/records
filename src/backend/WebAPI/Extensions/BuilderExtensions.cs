using Application.Configurations.EnvironmentVariables;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace WebAPI.Extensions;
 
public static class BuilderExtensions
{
    private static readonly StringEnvironmentVariable LogLevelEnv = new("LOG_LEVEL", false, "INFO");
     
    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        var logLevel = GetLogLevel();

        var logConfiguration = new LoggerConfiguration()
            .WriteTo.Console(new JsonFormatter(null, true, null))
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

    private static LogEventLevel GetLogLevel()
    {
        LogLevelEnv.GetEnvironmentVariable();
        LogEventLevel level;
        if (!Enum.TryParse<LogEventLevel>(LogLevelEnv.Value, out level))
            level = LogEventLevel.Information;
        return level;
    }
}