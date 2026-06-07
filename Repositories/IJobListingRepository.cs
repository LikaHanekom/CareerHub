using CareerHub.Api.Models;
using CareerHub.Api.DTOs;

namespace CareerHub.Api.Repositories
{
    public interface IJobListingRepository
    {
        // Must return actual collections/DTOs, never IQueryable<T> [cite: 41]
        Task<IEnumerable<JobListing>> GetActiveListingsWithCompanyAsync(); 
        Task<JobListing?> GetByIdAsync(Guid id);
        Task<bool> DoesListingExistAsync(string title, Guid companyId);
        Task AddAsync(JobListing listing);
        Task UpdateAsync(JobListing listing);
        Task DeleteAsync(JobListing listing);
        Task<bool> IsListingOpenAsync(Guid id);
        Task<IEnumerable<JobResponse>> SearchAsync(string searchTerm);
        IAsyncEnumerable<JobListing> GetListingsByCompanyCompiled(Guid companyId);

        Task<IEnumerable<JobListingStatsResponse>> GetApplicationStatsAsync(Guid companyId);
    }
}