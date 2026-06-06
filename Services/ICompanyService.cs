using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerHub.Api.Models; 
using CareerHub.Api.DTOs;

namespace CareerHub.Api.Services
{
    public interface ICompanyService
    {
        Task<IEnumerable<Company>> GetAllCompaniesAsync();
        Task<Company?> GetCompanyByIdAsync(Guid id);
        
        Task<Company> CreateCompanyAsync(CreateCompanyDto dto);
        
        Task<bool> DoesCompanyExistByNameAsync(string name);
    }
}