using System;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using CareerHub.Api.Enums;

namespace CareerHub.Api.Models;

public class Application
{
    public Guid JobListingId { get; set; }

    public Guid ApplicantId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Reviewing;

    public JobListing JobListing { get; set; } = null!;

    public Applicant Applicant { get; set; } = null!;
}