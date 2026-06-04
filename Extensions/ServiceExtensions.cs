using Microsoft.Extensions.DependencyInjection;
using CareerHub.Api.Services;

namespace CareerHub.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register JobServers
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IApplicantService, ApplicantService>();

        return services;
    }
}