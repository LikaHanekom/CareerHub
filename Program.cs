using Scalar.AspNetCore;
using CareerHub.Api.Services;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// 1. Configure the global Problem Details pipeline (Assignment Part 4 requirement)
builder.Services.AddProblemDetails();

// 2. Register Controllers and configure Enums to serialize as Strings instead of numbers
builder.Services.AddControllers() 
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register your single data service instance
builder.Services.AddSingleton<JobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Generates the JSON file at /openapi/v1.json
    
    // Updated syntax for Scalar 2.0+
    app.MapScalarApiReference(options =>
    {
        options.OpenApiRoutePattern = "/openapi/v1.json";
    });
}

// Automatically formats standard error codes (like 400 or 404) into rich Problem Details JSON objects
app.UseStatusCodePages();

app.UseHttpsRedirection();

// 3. MAP CONTROLLERS: This replaces app.MapGet, app.MapPost, etc.
app.MapControllers(); 

app.Run();