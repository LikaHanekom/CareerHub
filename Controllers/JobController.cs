using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CareerHub.Api.DTOs;
using CareerHub.Api.Exceptions;
using CareerHub.Api.Services;

namespace CareerHub.Api.Controllers;

[ApiController]
[Route("jobs")] 
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _jobService.CreateJobAsync(request);
        
        // Returns a clean 201 Created and directly passes back your response DTO data
        return StatusCode(201, result);
    }

    // ── 4. PUT /jobs/{id} (UPDATE) ────────────────────────────────────
    [Authorize(Roles = "Employer")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobResponse>> UpdateJobAsync(Guid id, [FromBody] UpdateJobRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        // One call straight down to the service layer
        var results = await _jobService.SearchJobsAsync(q);
        return Ok(results);
    }
}