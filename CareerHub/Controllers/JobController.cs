using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using CareerHub.Api.DTOs;
using CareerHub.Api.Exceptions;
using CareerHub.Api.Services;
using CareerHub.Api.Models;
using Microsoft.AspNetCore.RateLimiting;


namespace CareerHub.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/jobs")]
[ApiVersion("1.0")]
public class JobController(IJobService jobService) : ControllerBase
{
    private readonly IJobService _jobService = jobService;

    // ── 1. GET ALL JOBS (GET /jobs) ──────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<PagedResponse<JobResponse>>> GetActiveJobs(
       [FromQuery] JobListingFilterQuery filter)
    {
        // Pass the entire query parameter package to service layer 
        var pagedEnvelope = await _jobService.GetActiveJobsAsync(filter);

        // Write metadata tracking header out using  envelope data
        Response.Headers.Append("X-Total-Count", pagedEnvelope.TotalCount.ToString());

        // Return the smart math envelope payload
        return Ok(pagedEnvelope);
    }
    // ── 2. GET JOB BY ID (GET /jobs/{id}) ─────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetJobByIdAsync(Guid id) // 🚀 Changed to IActionResult
    {
        // Fetch data payload via service tier
        var jobDetail = await _jobService.GetJobByIdAsync(id);
        if (jobDetail == null)
        {
            throw new JobNotFoundException(id);
        }

        // Generate the structural execution fingerprint (ETag)
        string rawEtag = $"{jobDetail.Id}:{jobDetail.PostedAt.Ticks}:{jobDetail.SalaryMin}"; 
        string etag = $"\"{rawEtag}\""; // Standard HTTP double-quote wrappers

        // Inbound request caching headers
        if (Request.Headers.TryGetValue("If-None-Match", out var clientEtag) && clientEtag == etag) 
        {
            return StatusCode(StatusCodes.Status304NotModified); 
        }

        Response.Headers.ETag = etag; 
        return Ok(jobDetail); 
    }

    // ── 3. POST /jobs (CREATE) ────────────────────────────────────────
    [Authorize(Roles = "Employer")]
    [HttpPost]
    [EnableRateLimiting("post-listing")]
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
    [EnableRateLimiting("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        // One call straight down to the service layer
        var results = await _jobService.SearchJobsAsync(q);
        return Ok(results);
    }


    [HttpGet("company/{companyId:guid}/compiled")]
    public IAsyncEnumerable<JobListing> GetCompanyJobsCompiled(Guid companyId)
    {
        // Execution rows straight out to the client
        return _jobService.GetCompanyJobsCompiledAsync(companyId);
    }

    //  WINDOW FUNCTION ANALYTICS REPORT (GET /jobs/company/{companyId}/stats) 
    [Authorize(Roles = "Employer")]
    [HttpGet("company/{companyId:guid}/stats")]
    public async Task<ActionResult<IEnumerable<JobListingStatsResponse>>> GetCompanyStats(Guid companyId)
    {
        var reportData = await _jobService.GetCompanyApplicationStatsAsync(companyId);
        return Ok(reportData);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Employer,Admin")] 
    public async Task<ActionResult<JobResponse>> PatchJob(Guid id, [FromBody] UpdateJobListingRequest request)
    {
        var response = await _jobService.PatchJobAsync(id, request);
        return Ok(response);
    }
}