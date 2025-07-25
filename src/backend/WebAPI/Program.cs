using Application.Extensions;
using Application.Features;
using Persistence;
using WebAPI;
using WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.ConfigureLogging();

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = Configuration.JsonSerializerConfig.ContractResolver;
    });

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add data store services
builder.Services.AddPersistenceServices();

// Add core application logic services
builder.Services.AddFeatureServices();

builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureApiExtensions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();