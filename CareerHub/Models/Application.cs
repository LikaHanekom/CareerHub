using System;
using CareerHub.Api.Enums;

namespace CareerHub.Api.Models;

public class Application
{
    public Guid Id { get; set; }

    public Guid JobListingId { get; set; }

    public Guid ApplicantId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted; 

    public JobListing JobListing { get; set; } = null!;

    public Applicant Applicant { get; set; } = null!;
}