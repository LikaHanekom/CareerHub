using Scalar.AspNetCore;
using CareerHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using CareerHub.Api.Middleware;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Text;
using Microsoft.Extensions.Options;
using CareerHub.Api.Data;
using Microsoft.EntityFrameworkCore;
using CareerHub.Api.Extensions;
using CareerHub.Api.Infrastructure;
using Asp.Versioning;

// LoggerConfiguration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddInfrastructureServices(builder.Configuration);

    //register versioning engine
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0); 
        options.AssumeDefaultVersionWhenUnspecified = true; 
        options.ReportApiVersions = true; 
        options.ApiVersionReader = new UrlSegmentApiVersionReader(); 
    });
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

    builder.Services.AddRateLimiter(options =>
    {
        // Immediate rejection
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Custom Response Payload Construction
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                await context.HttpContext.Response.WriteAsync(
                    $"Rate limit exceeded. Please retry after {(int)retryAfter.TotalSeconds} seconds.", token);
            }
            else
            {
                await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", token);
            }
        };

        // Global Baseline Policy - 200 requests every 60 seconds
        options.AddFixedWindowLimiter("global", opt =>
        {
            opt.PermitLimit = 200;
            opt.Window = TimeSpan.FromSeconds(60);
            opt.QueueLimit = 0;
        });

        // Search Optimization Policy 
        options.AddSlidingWindowLimiter("search", opt =>
        {
            opt.PermitLimit = 30;
            opt.Window = TimeSpan.FromSeconds(60);
            opt.SegmentsPerWindow = 6; // Evaluates traffic blocks every 10 seconds smoothly
            opt.QueueLimit = 0;
        });

        // Application Protection Policy- Max 5 job applications perHour
        options.AddFixedWindowLimiter("apply", opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(60);
            opt.QueueLimit = 0;
        });

        //Listing Creation Spam Policy - Max 10 job posts per hour per user 
        options.AddFixedWindowLimiter("post-listing", opt =>
        {
            opt.PermitLimit = 10;
            opt.Window = TimeSpan.FromMinutes(60);
            opt.QueueLimit = 0;
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
    app.UseRateLimiter();

    // Pipeline ordering: Exception handler goes early to catch downstream errors
    app.UseExceptionHandler();

    app.UseAuthentication();//Checks who user is

    app.UseAuthorization();//check what the user is allowed to do
    
    app.UseStatusCodePages();
    app.UseHttpsRedirection();
    app.MapControllers(); 
    app.MapControllers().RequireRateLimiting("global");

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