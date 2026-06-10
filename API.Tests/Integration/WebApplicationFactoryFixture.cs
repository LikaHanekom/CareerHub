using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CareerHub.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Testcontainers.PostgreSql;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; 
using System.IO; 

namespace API.Tests.Integration;

public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("CareerHubTestDB")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Add configuration for test environment
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific appsettings
            config.AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: false);
            
            // Or add in-memory configuration as fallback
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSecretKeyForIntegrationTests12345678901234567890",
                ["Jwt:Issuer"] = "CareerHubTest",
                ["Jwt:Audience"] = "CareerHubTest"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            services.RemoveAll(typeof(DbContextOptions<CareerHubDbContext>));
            
            // Use the Testcontainer PostgreSQL connection string
            services.AddDbContext<CareerHubDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
            
            // Build service provider and ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CareerHubDbContext>();
            
            context.Database.Migrate();
        });
        
        builder.UseEnvironment("Test");
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.StopAsync();
        await _postgresContainer.DisposeAsync();
    }
}