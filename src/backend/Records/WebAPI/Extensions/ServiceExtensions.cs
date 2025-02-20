using Application.Configuration.EnvironmentVariables;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using WebAPI.Controllers.ControllerExtensions;

namespace WebAPI.Extensions;

public static class ServiceExtensions
{
    public static readonly BoolEnvironmentVariable EnableOpenTelemetryEnv = new("RECORDS__ENABLE_OPENTELEMETRY", false, false);
    private static readonly StringEnvironmentVariable OpenTelemetryUrlEnv = new("OTEL_EXPORTER_OTLP_ENDPOINT", false);

    public static readonly IntEnvironmentVariable PrometheusScrapePortEnv =
        new("RECORDS__PROMETHEUS_SCRAPE_PORT", false, 8081); 
    public static void ConfigureApiExtensions(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder => builder.
                AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });

        services.AddScoped<ClaimsInformation>();
    }
    
    public static void ConfigureOpenTelemetry(this IServiceCollection services)
    {
        var errors = new List<EnvironmentVariable>();
        EnableOpenTelemetryEnv.GetEnvironmentVariable();
        if (!EnableOpenTelemetryEnv.IsValid)
            EnvironmentVariable.PrintMissingEnvironmentVariablesAndExit([EnableOpenTelemetryEnv]);
     
        if (!EnableOpenTelemetryEnv.Value)
        {
            Log.Logger.Information("OpenTelemetry is disabled"); 
            return;
        }
     
        OpenTelemetryUrlEnv.GetEnvironmentVariable();
        if (!OpenTelemetryUrlEnv.IsValid)
            errors.Add(OpenTelemetryUrlEnv);
        
        PrometheusScrapePortEnv.GetEnvironmentVariable();
        if (!PrometheusScrapePortEnv.IsValid)
            errors.Add(PrometheusScrapePortEnv);
        
        if (errors.Any())
            EnvironmentVariable.PrintMissingEnvironmentVariablesAndExit(errors);
     
        if (EnableOpenTelemetryEnv.Value && OpenTelemetryUrlEnv.Value is null)
        {
            Console.WriteLine("environment variable 'APP__ENABLE_OPENTELEMETRY' is true but environment variable 'OTEL_EXPORTER_OTLP_ENDPOINT' is missing.");
            Environment.Exit(1);
        }
     
        Log.Logger.Information("OpenTelemetry is enabled");

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("Records"))
            .WithMetrics(metrics =>
            { 
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddNpgsqlInstrumentation()
                    .AddPrometheusExporter();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddNpgsql()
                    .AddOtlpExporter();
            });
    }
}
