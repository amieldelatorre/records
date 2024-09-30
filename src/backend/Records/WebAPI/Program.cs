using Application;
using Persistence;
using Serilog;
using WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.ConfigureLogging();
Log.Logger.Information("performing Web API startup configuration");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPersistenceServices();
builder.Services.AddUserFeatures();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureBuilder();

app.UseAuthorization();

app.MapControllers();

Log.Logger.Information("startup configuration successful, starting application");

app.Run();