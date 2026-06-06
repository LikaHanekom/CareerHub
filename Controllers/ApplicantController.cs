using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerHub.Api.Models;
using CareerHub.Api.Services;
using CareerHub.Api.Exceptions; 
using CareerHub.Api.DTOs; 
using Microsoft.AspNetCore.Mvc;

namespace CareerHub.Api.Controllers;

[ApiController]
[Route("applicants")] 
public class ApplicantController(IApplicantService applicantService) : ControllerBase
{
    private readonly IApplicantService _applicantService = applicantService;

    // ── 1. GET ALL APPLICANTS (GET /applicants) ───────────────────────────
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Applicant>>> GetAllAsync()
    {
        var applicants = await _applicantService.GetAllApplicantsAsync();
        return Ok(applicants);
    }

    // ── 2. GET APPLICANT BY ID (GET /applicants/{id}) ─────────────────────
    [HttpGet("{id:guid}")] 
    public async Task<ActionResult<Applicant>> GetByIdAsync(Guid id)
    {
        var applicant = await _applicantService.GetApplicantByIdAsync(id);
        if (applicant == null) 
        {
            
            throw new ApplicantNotFoundException(id); 
        }
        
        return Ok(applicant);
    }

    // ── 3. POST /applicants (CREATE) ──────────────────────────────────────
    [HttpPost]
    public async Task<ActionResult<Applicant>> CreateAsync([FromBody] CreateApplicant dto)
    {
        // Framework validation 
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdApplicant = await _applicantService.CreateApplicantAsync(dto);
        
    
        return CreatedAtAction(nameof(GetByIdAsync), new { id = createdApplicant.Id }, createdApplicant);
    }
}