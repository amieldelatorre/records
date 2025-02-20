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
    private static readonly BoolEnvironmentVariable EnableOpenTelemetryEnv = new("RECORDS__ENABLE_OPENTELEMETRY", false, false);
    private static readonly StringEnvironmentVariable OpenTelemetryUrlEnv = new("OTEL_EXPORTER_OTLP_ENDPOINT", false);
    
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
             EnableOpenTelemetryEnv.GetEnvironmentVariable();
             OpenTelemetryUrlEnv.GetEnvironmentVariable();
     
             if (!EnableOpenTelemetryEnv.Value)
             {
                 Log.Logger.Information("OpenTelemetry is disabled");
                 return;
             }
             else if (EnableOpenTelemetryEnv.Value && OpenTelemetryUrlEnv.Value is null)
             {
                 Console.WriteLine(
                     "environment variable 'APP__ENABLE_OPENTELEMETRY' is true but environment variable 'OTEL_EXPORTER_OTLP_ENDPOINT' is missing.");
                 Environment.Exit(0);
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
                         .AddOtlpExporter();
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
