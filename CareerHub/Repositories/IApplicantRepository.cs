using CareerHub.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CareerHub.Api.Repositories
{
    public interface IApplicantRepository
    {
        Task<IEnumerable<Applicant>> GetAllApplicantsAsync();
        Task<Applicant?> GetApplicantByIdAsync(Guid id);
        Task<Applicant> CreateApplicantAsync(Applicant applicant);
        Task<bool> DoesApplicantExistByEmailAsync(string email);
    }
}