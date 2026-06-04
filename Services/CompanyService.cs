using CareerHub.Api.Data;
using CareerHub.Api.Models;
using CareerHub.Api.DTOs;       
using CareerHub.Api.Exceptions; 
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly CareerHubDbContext _context;

        public CompanyService(CareerHubDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
        {
            return await _context.Companies.ToListAsync();
        }

        public async Task<Company?> GetCompanyByIdAsync(Guid id)
        {
            return await _context.Companies.FindAsync(id);
        }

        public async Task<Company> CreateCompanyAsync(CreateCompanyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Company name cannot be empty.");
            }

        
            var exists = await _context.Companies
                .AnyAsync(c => c.Name != null && c.Name.ToLower() == dto.Name.ToLower());

            if (exists)
            {
                throw new DuplicateCompanyException(dto.Name);
            }

            var company = new Company
            {
                Id = Guid.NewGuid(), 
                Name = dto.Name,
                Website = dto.Website, 
                JobListings = new List<JobListing>() 
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }
    }
}