using Scalar.AspNetCore;
using CareerHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using CareerHub.Api.Middleware;
using Serilog;

// 1. Configure the LoggerConfiguration at the very top
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Configure the global Problem Details pipeline
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // Register Controllers and configure Enums to serialize as Strings
    builder.Services.AddControllers() 
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    builder.Services.AddOpenApi();
    builder.Services.AddSingleton<JobService>();

    var app = builder.Build();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.OpenApiRoutePattern = "/openapi/v1.json";
        });
    }

    // Pipeline ordering: Exception handler goes early to catch downstream errors
    app.UseExceptionHandler();

    // Request logging goes after exception handling so it accurately logs status codes
    app.UseSerilogRequestLogging();
    
    app.UseStatusCodePages();
    app.UseHttpsRedirection();
    app.MapControllers(); 

    app.Run();
}
catch (Exception ex)
{
    // This catches any catastrophic failures during application startup
    Log.Fatal(ex, "Application terminated unexpectedly during startup.");
}
finally
{
    // Ensures any buffered log statements are written out before the process exits
    Log.CloseAndFlush();
}