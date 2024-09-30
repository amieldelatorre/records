namespace WebAPI.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureApiExtensions(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder => builder.
                AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });
    }

    public static void ConfigureBuilder(this IApplicationBuilder app)
    {
        app.UseCors();
        app.UseMiddleware<ExceptionMiddleware>();
    }
}