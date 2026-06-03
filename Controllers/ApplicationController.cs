using CareerHub.Api.Data;
using CareerHub.Api.DTOs;
using CareerHub.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationController(CareerHubDbContext dbContext) : ControllerBase
{
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyForJob([FromBody] ApplicationRequest request)
    {
        // Check for duplicates
        var alreadyApplied = await dbContext.Applications
            .AnyAsync(a => a.JobListingId == request.JobListingId && a.ApplicantId == request.ApplicantId);

        if (alreadyApplied)
        {
            return BadRequest(new { message = "Applicant has already applied for this job." });
        }

        var application = new Application
        {
            JobListingId = request.JobListingId,
            ApplicantId = request.ApplicantId,
            SubmittedAt = DateTime.UtcNow 
        };

        dbContext.Applications.Add(application);
        await dbContext.SaveChangesAsync();
        return StatusCode(201);
    }
}