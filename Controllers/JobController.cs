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

        // N+1 - Eager Loading
        var jobs = await _dbContext.JobListings
            .Include(j => j.Company)
            .ToListAsync();

        
        var response = jobs.Select(MapToResponse).ToList();

        return Ok(response);
        /*var response = await _dbContext.JobListings
            .AsNoTracking()
            .Select(j => new JobResponse
            {
                Id = j.Id,
                Title = j.Title,
                Location = j.Location,
                Description = j.Description,
                Type = j.Type,
                PostedAt = j.PostedAt,
                IsActive = j.IsActive,
                Company = j.Company.Name, // Maps the string field from navigation safely
                
                // Count is calculated directly inside the database via SQL COUNT()
                ApplicationCount = j.Applications.Count() 
            })
            .ToListAsync();

        return Ok(response);*/
    }

    // ── 2. GET JOB BY ID (GET /jobs/{id}) ─────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobResponse>> GetJobByIdAsync(Guid id)
    {
        var jobDetail = await _dbContext.JobListings
            .AsNoTracking()
            .Where(j => j.Id == id)
            .Select(j => new JobDetailResponse
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Location = j.Location,
                Type = j.Type,
                PostedAt = j.PostedAt,
                IsActive = j.IsActive,
                CompanyName = j.Company.Name,
                
                // Pulls only the name strings out of the join table
                AppliedApplicantNames = j.Applications
                    .Select(ap => ap.Applicant.FullName)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (jobDetail == null)
        {
            throw new JobNotFoundException(id);
        }

        return Ok(jobDetail);
    }

    // ── 3. POST /jobs (CREATE) ────────────────────────────────────────
    [Authorize(Roles = "Employer")]
    [HttpPost]
    
    public async Task<ActionResult<JobResponse>> CreateJobAsync([FromBody] CreateJobRequest request)
    {
        //  Duplicate Check using DbContext
        var exists = await _dbContext.JobListings.AnyAsync(j => j.Title == request.Title &&j.CompanyId == request.CompanyId);
        if (exists)
        {
           throw new DuplicateJobListingException(request.CompanyId.ToString(), request.Title);
        }


        // Create entity directly with DbContext
        var newJob = new JobListing
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            CompanyId = request.CompanyId,
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
        job.CompanyId = request.CompanyId;
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
            Company = job.Company.Name,
            Location = job.Location,
            Description = job.Description,
            Type = job.Type,
            PostedAt = job.PostedAt,
            IsActive = job.IsActive
        };
    }
}