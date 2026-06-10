using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerHub.Api.Models; 
using CareerHub.Api.DTOs;
using CareerHub.Api.Repositories;
using CareerHub.Api.Exceptions; 
namespace CareerHub.Api.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepo;

        // Injecting the repository interface to maintain strict decoupling
        public CompanyService(ICompanyRepository companyRepo)
        {
            _companyRepo = companyRepo;
        }

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
        {
            return await _companyRepo.GetAllCompaniesAsync();
        }

        public async Task<Company?> GetCompanyByIdAsync(Guid id)
        {
            return await _companyRepo.GetCompanyByIdAsync(id);
        }

        public async Task<Company> CreateCompanyAsync(CreateCompanyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Company name cannot be empty.");
            }

            // Business Validation Rule: Prevent creating duplicate companies by name
            var exists = await _companyRepo.DoesCompanyExistByNameAsync(dto.Name);
            if (exists)
            {
                // Throws a clean, trackable business exception handled by your GlobalExceptionHandler
                throw new DuplicateJobListingException(dto.Name); 
            }

            // Map incoming DTO values to the database domain model tracking layout
            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                
            };

            
            return await _companyRepo.CreateCompanyAsync(company);
        }

        public async Task<bool> DoesCompanyExistByNameAsync(string name)
        {
            return await _companyRepo.DoesCompanyExistByNameAsync(name);
        }
    }
}