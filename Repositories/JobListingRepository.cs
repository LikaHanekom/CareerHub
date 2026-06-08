using CareerHub.Api.Data;
using CareerHub.Api.Models;
using CareerHub.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using CareerHub.Api.Enums;

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
            await _context.SaveChangesAsync(); 
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

        public async Task<IEnumerable<JobResponse>> SearchAsync(string searchTerm)
        {
            // 1. Fallback: If no search term is provided, return all active, unexpired jobs projected to DTO
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.JobListings
                    .AsNoTracking()
                    .Where(j => j.IsActive && (j.ExpiresAt == null || j.ExpiresAt > DateTime.UtcNow))
                    .Select(j => new JobResponse
                    {
                        Id = j.Id,
                        Title = j.Title,
                        Description = j.Description,
                        CompanyId = j.CompanyId,
                        Company = j.Company != null ? j.Company.Name : string.Empty,
                        IsActive = j.IsActive,  
                        ExpiresAt = j.ExpiresAt
                    })
                    .ToListAsync();
            }

            // 2. Format the text for Postgres tsquery partial-matching with stemming capabilities
            var formattedQuery = searchTerm.Trim().Replace(" ", " & ") + ":*";

            // 3. High-performance GIN index search query path
            return await _context.JobListings
                .AsNoTracking()
                .Where(j => j.IsActive && (j.ExpiresAt == null || j.ExpiresAt > DateTime.UtcNow))
                .Where(j => EF.Property<NpgsqlTypes.NpgsqlTsVector>(j, "SearchVector")
                    .Matches(EF.Functions.ToTsQuery("english", formattedQuery)))
                .Select(j => new JobResponse
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    CompanyId = j.CompanyId,
                    Company = j.Company != null ? j.Company.Name : string.Empty,
                    IsActive = j.IsActive, 
                    ExpiresAt = j.ExpiresAt
                })
                .ToListAsync();
        }

        public IAsyncEnumerable<JobListing> GetListingsByCompanyCompiled(Guid companyId)
        {
            return _compiledCompanyJobsQuery(_context, companyId);
        }

        public async Task<IEnumerable<JobListingStatsResponse>> GetApplicationStatsAsync(Guid companyId)
        {
        //uards against SQL injection attacks
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

        public async Task<(IEnumerable<JobListing> Items, int TotalCount)> GetActiveListingsPagedAsync(int page, int pageSize)
        {
            // Establish a single, reusable query base 
            var query = _context.JobListings
                                .Where(j => j.IsActive); 

            // 1. Issue the Count query
            int totalCount = await query.CountAsync(); 

            // 2. Issue the Data fetch query with deterministic sort before Skip/Take 
            var items = await query.OrderByDescending(j => j.PostedAt) 
                                .Skip((page - 1) * pageSize) //remember pageSize is the amount of joblistigns per page
                                .Take(pageSize) 
                                .ToListAsync(); 

            return (items, totalCount);
        }

        public async Task<(IEnumerable<JobListing> Items, int TotalCount)> GetActiveListingsPagedAsync(JobListingFilterQuery filter)
        {
            // Establish the query root 
            var query = _context.JobListings.Where(j => j.IsActive).AsQueryable(); 

            // Dynamically compose WHERE clauses on the expression tree
            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                query = query.Where(j => EF.Functions.ILike(j.Location, $"%{filter.Location}%")); 
            }

            if (Enum.TryParse<JobType>(filter.EmploymentType, ignoreCase: true, out var parsedType))
            {
                query = query.Where(j => j.Type == parsedType); 
            }

            if (filter.SalaryMin.HasValue)
            {
                query = query.Where(j => j.SalaryMin >= filter.SalaryMin.Value); 
            }

            if (filter.SalaryMax.HasValue)
            {
                query = query.Where(j => j.SalaryMax <= filter.SalaryMax.Value); 
            }

            if (filter.CompanyId.HasValue)
            {
                query = query.Where(j => j.CompanyId == filter.CompanyId.Value); 
            }

            // Count matching rows before pagination
            int totalCount = await query.CountAsync(); 

            // Dynamic Sorting Evaluation 
            bool isAscending = filter.Dir.Equals("asc", StringComparison.OrdinalIgnoreCase); 
            
            query = filter.Sort.ToLower() switch
            {
                "salarymin" => isAscending ? query.OrderBy(j => j.SalaryMin) : query.OrderByDescending(j => j.SalaryMin), 
                "salarymax" => isAscending ? query.OrderBy(j => j.SalaryMax) : query.OrderByDescending(j => j.SalaryMax), 
                "title"     => isAscending ? query.OrderBy(j => j.Title) : query.OrderByDescending(j => j.Title), 
                _           => query.OrderByDescending(j => j.PostedAt) 
            };

            // Slice boundaries over the database using parameters inside the object
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(); 

            return (items, totalCount);
        }
    }
}