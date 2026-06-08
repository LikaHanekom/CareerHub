using Scalar.AspNetCore;
using CareerHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using CareerHub.Api.Middleware;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using CareerHub.Api.Data;
using Microsoft.EntityFrameworkCore;
using CareerHub.Api.Extensions;
using CareerHub.Api.Infrastructure;

// LoggerConfiguration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Serilog
    builder.Host.UseSerilog();

    //Global Problem Details pipeline
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // Register Controllers and configure Enums to serialize as Strings
    builder.Services.AddControllers() 
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // Scalar / OpenAPI configuration
    builder.Services.AddOpenApi();

    // 1. Enforce strict build-time Dependency Injection validation
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;       
        options.ValidateOnBuild = true;     
    });

    //Scalar configuration
    builder.Services.AddAuthFeatures();
    builder.Services.AddApplicantFeatures();
    builder.Services.AddJobFeatures();
    builder.Services.AddCompanyFeatures();
    builder.Services.AddApplicationFeatures();

    builder.Services.AddControllers();

    //Builder.config, tool to read configuration settings
    var jwtKey = builder.Configuration["Jwt:Key"]; //goes to fetch a secret configuration from the app confic files.
    var key = Encoding.UTF8.GetBytes(jwtKey!);// translation

    builder.Services.AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

    builder.Services.AddAuthorization();

    //Cors configuration
    builder.Services.AddCors(options =>
    {
       options.AddPolicy("FrontendPolicy", 
       policy =>
       {
            /*policy.AllowAnyOrigin().AllowCredentials()//testing line*/
            policy.WithOrigins("http://localhost:3000")//JS will rely on this
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("X-Total-Count");
       });
    });


    //register EF core
    builder.Services.AddDbContext<CareerHubDbContext>(options =>
    {
        options.UseNpgsql("Host=localhost;Port=5433;Database=CareerHub;Username=postgres;Password=Password123");
    });
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

    //Order matters
    // Request logging goes after exception handling so it accurately logs status codes
    app.UseSerilogRequestLogging();

    app.UseCors("FrontendPolicy");

    // Pipeline ordering: Exception handler goes early to catch downstream errors
    app.UseExceptionHandler();

    app.UseAuthentication();//Checks who user is

    app.UseAuthorization();//check what the user is allowed to do
    
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