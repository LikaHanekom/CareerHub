using CareerHub.Api.Models;
using CareerHub.Api.Services;
using CareerHub.Api.Exceptions; 
using CareerHub.Api.DTOs; 
using Microsoft.AspNetCore.Mvc;

namespace CareerHub.Api.Controllers
{
    [ApiController]
    [Route("companies")] 
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        // Constructor Injection
        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("{id:guid}")] 
        public async Task<IActionResult> GetById(Guid id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null) 
            {
                throw new CompanyNotFoundException(id); 
            }
            
            return Ok(company);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)//CreateCompanyDto
        {
            // Now this perfectly matches the signature in your CompanyService!
            var createdCompany = await _companyService.CreateCompanyAsync(dto);
            
            return CreatedAtAction(nameof(GetById), new { id = createdCompany.Id }, createdCompany);
        }
    }
}