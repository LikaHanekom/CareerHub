using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CareerHub.Api.DTOs;
using CareerHub.Api.Models;
using CareerHub.Api.Enums;
using CareerHub.Api.Services;

namespace CareerHub.Api.Controllers;

[ApiController]
[Route("api/applications")] 
public class ApplicationController(IApplicationService applicationService) : ControllerBase
{
    private readonly IApplicationService _applicationService = applicationService;

    // ── 1. SUBMIT AN APPLICATION 
    [HttpPost("apply")]
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

    // ── 2. Update ──────
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