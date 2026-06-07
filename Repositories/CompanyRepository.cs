using CareerHub.Api.Data;
using CareerHub.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareerHub.Api.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly CareerHubDbContext _context;

        public CompanyRepository(CareerHubDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
        {
            return await _context.Companies
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Company?> GetCompanyByIdAsync(Guid id)
        {
            return await _context.Companies
                .Include(c => c.JobListings)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> DoesCompanyExistByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            
            return await _context.Companies
                .AnyAsync(c => c.Name != null && c.Name.ToLower() == name.ToLower());
        }

        public async Task<Company> CreateCompanyAsync(Company company)
        {
            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();
            return company; 
        }
    }
}