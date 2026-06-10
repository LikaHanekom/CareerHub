using Microsoft.Extensions.DependencyInjection;
using CareerHub.Api.Services;
using CareerHub.Api.Repositories;
using CareerHub.Api.Infrastructure;
using CareerHub.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Extensions;

public static class ServiceExtensions
{

    public static IServiceCollection AddAuthFeatures(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>(); 
            return services;
        }

    //Applicant Features
    public static IServiceCollection AddApplicantFeatures(this IServiceCollection services)
    {
        // Register JobServers
        services.AddScoped<IApplicantRepository, ApplicantRepository>();
            services.AddScoped<IApplicantService, ApplicantService>();
            return services;
    }

    //Job Features
    public static IServiceCollection AddJobFeatures(this IServiceCollection services)
    {
        services.AddScoped<IJobListingRepository, JobListingRepository>();
        services.AddScoped<IJobService, JobService>();
        return services;
    }

    //Company Features
    public static IServiceCollection AddCompanyFeatures(this IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICompanyService, CompanyService>();
        return services;
    }

    //Application Features
    public static IServiceCollection AddApplicationFeatures(this IServiceCollection services)
    {
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IApplicationService, ApplicationService>();
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Register the interceptor as a stateless singleton
        services.AddSingleton<SlowQueryInterceptor>();

        // 2. Inject it into the DbContext options pipeline using the service provider (sp)
        services.AddDbContext<CareerHubDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<SlowQueryInterceptor>();
            
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(interceptor);
        });

        return services;
    }


}