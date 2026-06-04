using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using CareerHub.Api.Models;
using CareerHub.Api.DTOs;
using CareerHub.Api.Exceptions;
using CareerHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using CareerHub.Api.Data;

namespace CareerHub.Api.Controllers;

[ApiController]
[Route("jobs")] // This makes every endpoint in this file start with /jobs
public class JobController(IJobService jobService) : ControllerBase
{
    private readonly IJobService _jobService = jobService;


    // ── 1. GET ALL JOBS (GET /jobs) ──────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobResponse>>> GetAllJobsAsync()
    {

        var response = await _jobService.GetAllJobsAsync();
        return Ok(response);
    }

    // ── 2. GET JOB BY ID (GET /jobs/{id}) ─────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobResponse>> GetJobByIdAsync(Guid id)
    {
        var jobDetail = await _jobService.GetJobByIdAsync(id);
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
        var exists = await _jobService.ExistsAsync(request.Title, request.CompanyId);
        if (exists)
        {
            throw new DuplicateJobListingException(request.CompanyId.ToString(), request.Title);
        }

        var result = await _jobService.CreateJobAsync(request);
        return StatusCode(201, result);
    }

    // ── 4. PUT /jobs/{id} (UPDATE) ────────────────────────────────────
    [Authorize(Roles = "Employer")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobResponse>> UpdateJobAsync(Guid id, [FromBody] UpdateJobRequest request)
    {
        var updatedJob = await _jobService.UpdateJobAsync(id, request);
        if (updatedJob == null)
        {
            throw new JobNotFoundException(id);
        }

        return Ok(updatedJob);
    }

    // ── 5. DELETE /jobs/{id} (DELETE) ─────────────────────────────────
    [Authorize(Roles = "Employer")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteJobAsync(Guid id)
    {
        var deleted = await _jobService.DeleteJobAsync(id);
        if (!deleted)
        {
            throw new JobNotFoundException(id);
        }

        return NoContent(); 
    }
}