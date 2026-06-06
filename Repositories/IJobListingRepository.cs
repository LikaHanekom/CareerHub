using CareerHub.Api.Models;
using CareerHub.Api.DTOs;

namespace CareerHub.Api.Repositories
{
    public interface IJobListingRepository
    {
        // Must return actual collections/DTOs, never IQueryable<T> [cite: 41]
        Task<IEnumerable<JobResponse>> GetActiveListingsWithCompanyAsync(); 
        Task<JobListing?> GetByIdAsync(Guid id);
        Task<bool> DoesListingExistAsync(string title, Guid companyId);
        Task AddAsync(JobListing listing);
        Task UpdateAsync(JobListing listing);
        Task DeleteAsync(JobListing listing);
    }
}