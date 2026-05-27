using System;
using CareerHub.Api.Enums;
namespace CareerHub.Api.Models;

public class JobListing
{
    public int Id {get; set;}
    public string Title {get; set;} = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobType Type { get; set; }

    public DateTime PostedAt {get; set;}

    public bool IsActive {get; set;}

}