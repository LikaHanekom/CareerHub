using CareerHub.Api.Data;
using CareerHub.Api.Models;
using CareerHub.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Repositories
{
    public class JobListingRepository : IJobListingRepository
    {
        private readonly CareerHubDbContext _context;

        public JobListingRepository(CareerHubDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobResponse>> GetActiveListingsWithCompanyAsync()
        {
            return await _context.JobListings
                .AsNoTracking()
                .Select(j => new JobResponse
                {
                    Id = j.Id,
                    Title = j.Title,
                    Location = j.Location,
                    Description = j.Description,
                    Type = j.Type,
                    PostedAt = j.PostedAt,
                    IsActive = j.IsActive,
                    Company = j.Company != null ? j.Company.Name : "Unknown",
                    ApplicationCount = j.Applications.Count()
                })
                .ToListAsync();
        }

        public async Task<JobListing?> GetByIdAsync(Guid id)
        {
            // Return the raw entity so the service can check its properties or update it
            return await _context.JobListings
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<bool> DoesListingExistAsync(string title, Guid companyId)
        {
            return await _context.JobListings
                .AnyAsync(j => j.Title == title && j.CompanyId == companyId);
        }

        public async Task AddAsync(JobListing listing)
        {
            await _context.JobListings.AddAsync(listing);
            await _context.SaveChangesAsync(); // Handled internally [cite: 43]
        }

        public async Task UpdateAsync(JobListing listing)
        {
            _context.JobListings.Update(listing);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(JobListing listing)
        {
            _context.JobListings.Remove(listing);
            await _context.SaveChangesAsync();
        }
    }
}