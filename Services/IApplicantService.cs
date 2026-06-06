using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerHub.Api.Models; 
using CareerHub.Api.DTOs;

namespace CareerHub.Api.Services
{
    public interface IApplicantService
    {
        
        Task<IEnumerable<Applicant>> GetAllApplicantsAsync();
        Task<Applicant?> GetApplicantByIdAsync(Guid id);
        Task<Applicant> CreateApplicantAsync(CreateApplicant dto);

        
        Task<bool> DoesApplicantExistByEmailAsync(string email);
    }
}