using System;
using CareerHub.Api.Enums;

namespace CareerHub.Api.DTOs;

public class JobResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobType Type { get; set; }
    public DateTime PostedAt { get; set; }
    public bool IsActive { get; set; }
    
    // Optional raw numeric values used for mapping calculations
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }

    public int ApplicationCount { get; set; }

    // Computed property that converts numeric ranges into human-readable strings
    public string SalaryDisplay
    {
        get
        {
            if (SalaryMin.HasValue && SalaryMax.HasValue)
            {
                return $"R{SalaryMin.Value:N0}-R{SalaryMax.Value:N0}/month"; // e.g., R25,000-R40,000/month
            }
            if (SalaryMin.HasValue)
            {
                return $"From R{SalaryMin.Value:N0}/month"; // e.g., From R25,000/month
            }
            
            return "Salary not specified";
        }
    }
}