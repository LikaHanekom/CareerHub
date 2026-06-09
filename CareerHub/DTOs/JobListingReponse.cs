using System;
using System.Collections.Generic;
using CareerHub.Api.Enums;

namespace CareerHub.Api.DTOs;

public class JobDetailResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobType Type { get; set; }
    public DateTime PostedAt { get; set; }
    public bool IsActive { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    
    // Only expose a list of names 
    public List<string> AppliedApplicantNames { get; set; } = new();
    public List<ApplicationDetailResponse> Applications { get; set; } = new();
}