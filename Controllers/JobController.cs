using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using CareerHub.Api.Models;
using CareerHub.Api.DTOs;
using CareerHub.Api.Exceptions;
using Microsoft.AspNetCore.Authorization;
using CareerHub.Api.Data;

namespace CareerHub.Api.Controllers;

[ApiController]
[Route("jobs")] // This makes every endpoint in this file start with /jobs
public class JobController(CareerHubDbContext dbContext): ControllerBase
{
    private readonly CareerHubDbContext _dbContext = dbContext;//means Controller-DbContext-PostgrSQL


    // ── 1. GET ALL JOBS (GET /jobs) ──────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobResponse>>> GetAllJobsAsync()
    {
        var jobs = await _dbContext.JobListings.ToListAsync();//gets *FROM job_listings
        var response = jobs.Select(MapToResponse);
        return Ok(response);
    }

    // ── 2. GET JOB BY ID (GET /jobs/{id}) ─────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobResponse>> GetJobByIdAsync(Guid id)
    {
        var job = await _dbContext.JobListings.FindAsync(id);

        if (job == null)
        {
            throw new JobNotFoundException(id);
        }

        return Ok(MapToResponse(job));
    }

    // ── 3. POST /jobs (CREATE) ────────────────────────────────────────
    [Authorize(Roles = "Employer")]
    [HttpPost]
    
    public async Task<ActionResult<JobResponse>> CreateJobAsync([FromBody] CreateJobRequest request)
    {
        //  Duplicate Check using DbContext
        var exists = await _dbContext.JobListings
            .AnyAsync(j => j.Title == request.Title && j.Company == request.Company);

        if (exists)
        {
            throw new DuplicateJobListingException(request.Company,request.Title);
        }

        // Create entity directly with DbContext
        var newJob = new JobListing
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Company = request.Company,
            Location = request.Location,
            Description = request.Description,
            Type = request.Type,
            PostedAt = DateTime.UtcNow, 
            IsActive = true             
        };

        _dbContext.JobListings.Add(newJob);//only tells EF track this
        await _dbContext.SaveChangesAsync();//database write

        var response = MapToResponse(newJob);

        return StatusCode(201, MapToResponse(newJob));
    }

    // ── 4. PUT /jobs/{id} (UPDATE) ────────────────────────────────────
    [Authorize(Roles = "Employer")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobResponse>> UpdateJobAsync(Guid id, [FromBody] UpdateJobRequest request)
    {
        //  Find, throw if null, update properties, and save
        var job = await _dbContext.JobListings.FindAsync(id);

        if (job == null)
        {
            throw new JobNotFoundException(id);
        }

        job.Title = request.Title;
        job.Company = request.Company;
        job.Location = request.Location;
        job.Description = request.Description;
        job.Type = request.Type;

        await _dbContext.SaveChangesAsync();

        return Ok(MapToResponse(job));
    }

    // ── 5. DELETE /jobs/{id} (DELETE) ─────────────────────────────────
    [Authorize(Roles = "Employer")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteJobAsync(Guid id)
    {
        // Find, throw if null, remove, save, and return NoContent
        var job = await _dbContext.JobListings.FindAsync(id);

        if (job == null)
        {
            throw new JobNotFoundException(id);
        }

        _dbContext.JobListings.Remove(job);//mark for detetion
        await _dbContext.SaveChangesAsync();//delete

        return NoContent(); 
    }

    // Centralized mapping helper to map Domain Models safely to public Response Contracts
    private static JobResponse MapToResponse(JobListing job)
    {
        return new JobResponse
        {
            Id = job.Id,
            Title = job.Title,
            Company = job.Company,
            Location = job.Location,
            Description = job.Description,
            Type = job.Type,
            PostedAt = job.PostedAt,
            IsActive = job.IsActive
        };
    }
}