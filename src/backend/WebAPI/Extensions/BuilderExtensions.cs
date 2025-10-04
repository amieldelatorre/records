using System.Diagnostics;
using Application.Configurations.EnvironmentVariables;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace WebAPI.Extensions;
 
public static class BuilderExtensions
{
    private static readonly StringEnvironmentVariable LogLevelEnv = new("LOG_LEVEL", false, "INFO");
    private const string OtelExporterOtlpEndpointEnvVarName = "OTEL_EXPORTER_OTLP_ENDPOINT";
    private const string OtelExporterOtlpTracesEndpointEnvVarName = "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT";
    private const string OtelExporterOtlpMetricsEndpointEnvVarName = "OTEL_EXPORTER_OTLP_METRICS_ENDPOINT";
    
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

    public static void ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        Serilog.Log.Information("Configuring OTEL exporter(s)");
        var openTelemetryBuilder = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("Records", serviceInstanceId: Environment.MachineName));
        ConfigureOpenTelemetryMetrics(openTelemetryBuilder);
        ConfigureOpenTelemetryTraces(openTelemetryBuilder);
    }
    
    private static LogEventLevel GetLogLevel()
    {
        LogLevelEnv.GetEnvironmentVariable();
        LogEventLevel level;
        if (!Enum.TryParse<LogEventLevel>(LogLevelEnv.Value, out level))
            level = LogEventLevel.Information;
        return level;
    }

    private static void ConfigureOpenTelemetryMetrics(OpenTelemetryBuilder openTelemetryBuilder)
    {
        var otelExporterEndpoint = Environment.GetEnvironmentVariable(OtelExporterOtlpEndpointEnvVarName);
        var otelMetricsExporterEndpoint = Environment.GetEnvironmentVariable(OtelExporterOtlpMetricsEndpointEnvVarName);
        if (string.IsNullOrEmpty(otelExporterEndpoint) && string.IsNullOrEmpty(otelMetricsExporterEndpoint))
            return;

        var endpoint = !string.IsNullOrEmpty(otelMetricsExporterEndpoint) ? otelMetricsExporterEndpoint : otelExporterEndpoint;
        Debug.Assert(!string.IsNullOrEmpty(endpoint));
        
        Serilog.Log.Information("Configuring OTEL metrics exporter");
        openTelemetryBuilder.WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsqlInstrumentation();

            // Manually set the endpoint because the environment variable is not being honoured
            metrics.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(endpoint);
            });
        });
    }

    private static void ConfigureOpenTelemetryTraces(OpenTelemetryBuilder openTelemetryBuilder)
    {
        var otelExporterEndpoint = Environment.GetEnvironmentVariable(OtelExporterOtlpEndpointEnvVarName);
        var otelTracesExporterEndpoint = Environment.GetEnvironmentVariable(OtelExporterOtlpTracesEndpointEnvVarName);
        if  (string.IsNullOrEmpty(otelExporterEndpoint) && string.IsNullOrEmpty(otelTracesExporterEndpoint))
            return;

        var endpoint = !string.IsNullOrEmpty(otelTracesExporterEndpoint) ? otelTracesExporterEndpoint : otelExporterEndpoint;
        Debug.Assert(!string.IsNullOrEmpty(endpoint));
        
        Serilog.Log.Information("Configuring OTEL traces exporter");
        openTelemetryBuilder.WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsql();
            
            // Manually set the endpoint because the environment variable is not being honoured
            tracing.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(endpoint);
            });
        });
    }
}