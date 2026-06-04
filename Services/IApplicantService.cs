using CareerHub.Api.Models; 
using CareerHub.Api.DTOs;

namespace CareerHub.Api.Services
{
    public interface IApplicantService
    {
        Task<IEnumerable<Applicant>> GetAllApplicantsAsync();
        Task<Applicant?> GetApplicantByIdAsync(Guid id);
        Task<Applicant> CreateApplicantAsync(CreateApplicantDto dto);
    }
}