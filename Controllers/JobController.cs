using Microsoft.AspNetCore.Mvc;
using CareerHub.Api.Models;
using CareerHub.Api.DTOs;
using CareerHub.Api.Services;

namespace CareerHub.Api.Controllers;

[ApiController]
[Route("jobs")] // This makes every endpoint in this file start with /jobs
public class JobsController : ControllerBase
{
    private readonly JobService _jobService;

    // ASP.NET Core automatically injects your registered JobService here
    public JobsController(JobService jobService)
    {
        _jobService = jobService;
    }

    // ── 1. GET ALL JOBS (GET /jobs) ──────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobResponse>>> GetAllJobsAsync()
    {
        var jobs = await _jobService.GetAllJobsAsync();
        var response = jobs.Select(MapToResponse);
        return Ok(response);
    }

    // ── 2. GET JOB BY ID (GET /jobs/{id}) ─────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobResponse>> GetJobByIdAsync(int id)
    {
        var job = await _jobService.GetJobByIdAsync(id);

        if (job is null)
        {
            return Problem(
                detail: $"Job listing with ID {id} was not found.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found"
            );
        }

        return Ok(MapToResponse(job));
    }

    // ── 3. POST /jobs (CREATE) ────────────────────────────────────────
    [HttpPost]
    public async Task<ActionResult<JobResponse>> CreateJobAsync([FromBody] CreateJobRequest request)
    {
        // Duplicate check (case-insensitive) to prevent double-submits
        bool isDuplicate = await _jobService.ExistsAsync(request.Title, request.Company);
        if (isDuplicate)
        {
            return Problem(
                detail: $"A job listing for '{request.Title}' at '{request.Company}' already exists.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Duplicate Job Listing"
            );
        }

        // Map received request DTO parameters onto our server Domain Model
        var newJob = new JobListing
        {
            Title = request.Title,
            Company = request.Company,
            Location = request.Location,
            Description = request.Description,
            Type = request.Type,
            PostedAt = DateTime.UtcNow, // Server-owned timestamp
            IsActive = true             // Server-owned default state
        };

        var createdJob = await _jobService.CreateJobAsync(newJob);
        var response = MapToResponse(createdJob);

        // Success: 201 Created with a Location header pointing to GET /jobs/{id}
        return CreatedAtAction(nameof(GetJobByIdAsync), new { id = response.Id }, response);
    }

    // ── 4. PUT /jobs/{id} (UPDATE) ────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<ActionResult<JobResponse>> UpdateJobAsync(int id, [FromBody] UpdateJobRequest request)
    {
        var existingJob = await _jobService.GetJobByIdAsync(id);
        if (existingJob == null)
        {
            return Problem(
                detail: $"Job listing with ID {id} was not found.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found"
            );
        }

        var updatedJobData = new JobListing
        {
            Title = request.Title,
            Company = request.Company,
            Location = request.Location,
            Description = request.Description,
            Type = request.Type
        };

        var result = await _jobService.UpdateJobAsync(id, updatedJobData);
        return Ok(MapToResponse(result!));
    }

    // ── 5. DELETE /jobs/{id} (DELETE) ─────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteJobAsync(int id)
    {
        var wasDeleted = await _jobService.DeleteJobAsync(id);
        if (!wasDeleted)
        {
            return Problem(
                detail: $"Job listing with ID {id} does not exist.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found"
            );
        }

        return NoContent(); // HTTP 204 No Content for successful deletion
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