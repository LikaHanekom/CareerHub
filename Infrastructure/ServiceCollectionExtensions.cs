using Microsoft.Extensions.DependencyInjection;
using CareerHub.Api.Services;
using CareerHub.Api.Repositories;

namespace CareerHub.Api.Extensions;

public static class ServiceExtensions
{

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


}