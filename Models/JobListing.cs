using System;
using CareerHub.Api.Enums;
namespace CareerHub.Api.Models;

public class JobListing
{
    public Guid Id {get; set;}
    public string Title {get; set;} = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CompanyId { get; set; } //creates column in database table to hold the ID

    public Company Company { get; set; } = null!;//EF core use to help navigations from joblistigns to associated company

    public string Location { get; set; } = string.Empty;
    public JobType Type { get; set; }

    public DateTime PostedAt {get; set;} = DateTime.UtcNow;

    public decimal? SalaryMin { get; set; } 
    public decimal? SalaryMax { get; set; } 
    public DateTime? ExpiresAt { get; set; }

    public bool IsActive {get; set;}

    public ICollection<Application> Applications { get; set; } = new List<Application>();//establish other side of relationship

}