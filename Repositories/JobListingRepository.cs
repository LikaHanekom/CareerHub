using CareerHub.Api.Data;
using CareerHub.Api.Models;
using CareerHub.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Repositories
{
    public class JobListingRepository : IJobListingRepository
    {
        private readonly CareerHubDbContext _context;

        private static readonly Func<CareerHubDbContext, Guid, IAsyncEnumerable<JobListing>> _compiledCompanyJobsQuery =
            EF.CompileAsyncQuery((CareerHubDbContext context, Guid companyId) =>
                context.JobListings
                    .AsNoTracking() 
                    .Where(j => j.CompanyId == companyId));

        public JobListingRepository(CareerHubDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobListing>> GetActiveListingsWithCompanyAsync()
        {
            return await _context.JobListings
                .AsNoTracking()
                .Where(j => j.IsActive)
                .Include(j => j.Company) 
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

        public async Task<bool> IsListingOpenAsync(Guid id)
        {
            var job = await _context.JobListings
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null) return false;

            
            return job.IsActive; 
        }

        public async Task<IEnumerable<JobListing>> SearchAsync(string searchTerm)
        {
            // If no search term is provided, default to returning all active, unexpired jobs
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.JobListings
                    .AsNoTracking()
                    .Include(j => j.Company)
                    .Where(j => j.IsActive && (j.ExpiresAt == null || j.ExpiresAt > DateTime.UtcNow))
                    .ToListAsync();
            }

            // Format the text for Postgres tsquery partial-matching
            var formattedQuery = searchTerm.Trim().Replace(" ", " & ") + ":*";

            return await _context.JobListings
                .AsNoTracking()
                .Include(j => j.Company) // Includes the company data just like your other methods
                .Where(j => j.IsActive && (j.ExpiresAt == null || j.ExpiresAt > DateTime.UtcNow))
                
                // This links directly to the shadow property "SearchVector" and applies the GIN index
                .Where(j => EF.Property<NpgsqlTypes.NpgsqlTsVector>(j, "SearchVector")
                    .Matches(EF.Functions.ToTsQuery("english", formattedQuery)))
                .ToListAsync();
        }

        public IAsyncEnumerable<JobListing> GetListingsByCompanyCompiled(Guid companyId)
        {
            return _compiledCompanyJobsQuery(_context, companyId);
        }

        public async Task<IEnumerable<JobListingStatsResponse>> GetApplicationStatsAsync(Guid companyId)
    {
        //Quards against SQL injection attacks
        return await _context.Database.SqlQuery<JobListingStatsResponse>($@" 
            SELECT 
                j.""Id"" AS ""JobId"",
                j.""Title"" AS ""Title"",
                COUNT(a.""Id"") FILTER (WHERE a.""Status"" = 0)::int AS ""SubmittedCount"",
                COUNT(a.""Id"") FILTER (WHERE a.""Status"" = 1)::int AS ""UnderReviewCount"",
                COUNT(a.""Id"") FILTER (WHERE a.""Status"" = 2)::int AS ""ShortlistedCount"",
                COUNT(a.""Id"") FILTER (WHERE a.""Status"" = 3)::int AS ""RejectedCount"",
                COUNT(a.""Id"") FILTER (WHERE a.""Status"" = 4)::int AS ""OfferedCount"",
                COUNT(a.""Id"")::int AS ""TotalApplications"",
                RANK() OVER (ORDER BY COUNT(a.""Id"") DESC) AS ""Rank""
            FROM job_listings j
            LEFT JOIN applications a ON j.""Id"" = a.""JobListingId""
            WHERE j.""CompanyId"" = {companyId}
            GROUP BY j.""Id"", j.""Title""
        ").ToListAsync();
    }
    }
}