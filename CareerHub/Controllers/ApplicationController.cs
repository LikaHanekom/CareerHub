using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using CareerHub.Api.DTOs;
using CareerHub.Api.Models;
using CareerHub.Api.Enums;
using CareerHub.Api.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace CareerHub.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/applications")]
[ApiVersion("1.0")]
public class ApplicationController(IApplicationService applicationService) : ControllerBase
{
    private readonly IApplicationService _applicationService = applicationService;

    // ── 1. SUBMIT AN APPLICATION 
    [HttpPost("apply")]
    [EnableRateLimiting("apply")]
    public async Task<ActionResult<Application>> ApplyForJob([FromBody] ApplicationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Submits application via service tier. Relies on middleware if duplicate exists.
        var result = await _applicationService.SubmitApplicationAsync(request);

        return StatusCode(201, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetApplicationById(Guid id) // 🚀 Uses IActionResult for flexible 304/200 returns
    {
        // 1. Fetch the application using your service layer
        var app = await _applicationService.GetApplicationByIdAsync(id);
        if (app == null)
        {
            return NotFound($"Application with ID {id} was not found.");
        }

        // 2. Generate structural execution fingerprint using ID and Status enum
        string rawEtag = $"{app.Id}:{app.Status}";
        string etag = $"\"{rawEtag}\""; // Double quotes are required by HTTP specifications

        // 3. Evaluate inbound caching headers sent by the client
        if (Request.Headers.TryGetValue("If-None-Match", out var clientEtag) && clientEtag == etag)
        {
            // Cache Hit! Return a 304 Not Modified status with a completely empty body
            return StatusCode(StatusCodes.Status304NotModified);
        }

        // 4. Cache Miss: Append the computed ETag header to the response and return data
        Response.Headers.ETag = etag;
        return Ok(app);
    }

    // Update ──────
    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus(
        [FromQuery] Guid applicantId, 
        [FromQuery] Guid jobListingId, 
        [FromQuery] ApplicationStatus newStatus) 
    {
        await _applicationService.UpdateStatusAsync(applicantId, jobListingId, newStatus);
        return NoContent(); 
    }

    // ── 3.DELETE 
    [HttpDelete("{applicationId:guid}/Cancelled")]
    public async Task<IActionResult> WithdrawApplication(
        Guid applicationId, 
        [FromQuery] Guid requestingApplicantId)
    {
        // Service ensures the requesting applicant actually owns the application before removal
        await _applicationService.WithdrawApplicationAsync(applicationId, requestingApplicantId);

        return NoContent(); 
    }

    [HttpPatch("{id:guid}/status")] 
    public async Task<IActionResult> PatchStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            //Call  updated service tier
            var updatedApplication = await _applicationService.PartialUpdateStatusAsync(id, request);
            
            if (updatedApplication == null) 
                return NotFound($"Application with ID {id} was not found.");

            return Ok(updatedApplication);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}