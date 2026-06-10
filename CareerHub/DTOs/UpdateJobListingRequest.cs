namespace CareerHub.Api.DTOs;

public record UpdateJobListingRequest(
    string? Title = null, 
    string? Description = null, 
    string? Location = null, 
    string? EmploymentType = null, //Gets passed from frontend
    decimal? SalaryMin = null, 
    decimal? SalaryMax = null, 
    DateTime? ExpiresAt = null
);