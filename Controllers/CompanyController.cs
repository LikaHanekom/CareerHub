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
[Route("companies")] 
public class CompanyController(ICompanyService companyService) : ControllerBase
{
    private readonly ICompanyService _companyService = companyService;

    // ── 1. GET ALL COMPANIES 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Company>>> GetAllAsync()
    {
        var companies = await _companyService.GetAllCompaniesAsync();
        return Ok(companies);
    }

    // ── 2. GET COMPANY BY ID 
    [HttpGet("{id:guid}")] 
    public async Task<ActionResult<Company>> GetByIdAsync(Guid id)
    {
        var company = await _companyService.GetCompanyByIdAsync(id);
        if (company == null) 
        {
            // Intercepted and handled gracefully by your global exception middleware
            throw new CompanyNotFoundException(id); 
        }
        
        return Ok(company);
    }

    // ── 3. POST 
    [HttpPost]
    public async Task<ActionResult<Company>> CreateAsync([FromBody] CreateCompanyDto dto)
    {
        var createdCompany = await _companyService.CreateCompanyAsync(dto);
        
        return CreatedAtAction(nameof(GetByIdAsync), new { id = createdCompany.Id }, createdCompany);
    }
}