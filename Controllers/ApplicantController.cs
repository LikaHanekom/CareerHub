using CareerHub.Api.Models;
using CareerHub.Api.Services;
using CareerHub.Api.Exceptions; 
using CareerHub.Api.DTOs; 
using Microsoft.AspNetCore.Mvc;

namespace CareerHub.Api.Controllers
{
    [ApiController]
    [Route("applicants")] 
    public class ApplicantController : ControllerBase
    {
        private readonly IApplicantService _applicantService;

        public ApplicantController(IApplicantService applicantService)
        {
            _applicantService = applicantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var applicants = await _applicantService.GetAllApplicantsAsync();
            return Ok(applicants);
        }

        [HttpGet("{id:guid}")] 
        public async Task<IActionResult> GetById(Guid id)
        {
            var applicant = await _applicantService.GetApplicantByIdAsync(id);
            if (applicant == null) 
            {
                throw new ApplicantNotFoundException(id); 
            }
            
            return Ok(applicant);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApplicantDto dto)
        {
            var createdApplicant = await _applicantService.CreateApplicantAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdApplicant.Id }, createdApplicant);
        }
    }
}