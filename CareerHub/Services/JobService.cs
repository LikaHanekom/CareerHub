using CareerHub.Api.Models;
using CareerHub.Api.DTOs;
using CareerHub.Api.Repositories; 
using CareerHub.Api.Exceptions;

namespace CareerHub.Api.Services;

public class JobService : IJobService
{
    private readonly IJobListingRepository _repo;
    private readonly ICompanyRepository? _companyRepo; // Optional for backward compatibility

    // Original constructor - keeps existing code working
    public JobService(IJobListingRepository repo)
    {
        _repo = repo;
        _companyRepo = null;
    }

    // New constructor for tests and future use
    public JobService(IJobListingRepository repo, ICompanyRepository companyRepo)
    {
        _repo = repo;
        _companyRepo = companyRepo;
    }

    public async Task<IEnumerable<JobResponse>> GetAllJobsAsync()
    {
        var jobs = await _repo.GetActiveListingsWithCompanyAsync();

        var response = new List<JobResponse>();
    
        foreach (var job in jobs)
        {
            response.Add(new JobResponse
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                Type = job.Type,
                PostedAt = job.PostedAt,
                IsActive = job.IsActive,
                Company = job.Company?.Name ?? "Unknown" 
            });
        }
    
        return response;
    }

    public async Task<JobResponse?> GetJobByIdAsync(Guid id)
    {
        var job = await _repo.GetByIdAsync(id);
        if (job == null) return null;

        return new JobResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            Type = job.Type,
            PostedAt = job.PostedAt,
            IsActive = job.IsActive,
            Company = job.Company?.Name ?? "Unknown"
        };
    }

    public async Task<bool> ExistsAsync(string title, Guid companyId)
    {
        return await _repo.DoesListingExistAsync(title, companyId);
    }

    public async Task<JobResponse> CreateJobAsync(CreateJobRequest request)
    {
        // 1. Validate company exists (if company repository is available)
        if (_companyRepo != null)
        {
            var company = await _companyRepo.GetCompanyByIdAsync(request.CompanyId);
            if (company == null)
                throw new CompanyNotFoundException(request.CompanyId);
        }

        // 2. Validate salary range
        if (request.SalaryMax < request.SalaryMin)
            throw new InvalidSalaryException("Salary maximum cannot be less than salary minimum.");

        // 3. Validate expiration date
        if (request.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidListingException("Expiration date must be in the future.");

        // 4. Check for duplicates
        var duplicate = await _repo.DoesListingExistAsync(request.Title, request.CompanyId);
        if (duplicate) 
            throw new DuplicateJobListingException(request.Title);

        // 5. Create the new job with ALL fields
        var newJob = new JobListing
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            CompanyId = request.CompanyId,
            Location = request.Location,
            Description = request.Description,
            Type = request.Type,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            ExpiresAt = request.ExpiresAt,
            PostedAt = DateTime.UtcNow, 
            IsActive = true            
        };

        await _repo.AddAsync(newJob);

        // Get company name for response if available
        string companyName = "Unknown";
        if (_companyRepo != null)
        {
            var company = await _companyRepo.GetCompanyByIdAsync(request.CompanyId);
            companyName = company?.Name ?? "Unknown";
        }

        return new JobResponse 
        {
            Id = newJob.Id,
            Title = newJob.Title,
            Location = newJob.Location,
            Description = newJob.Description,
            Type = newJob.Type,
            SalaryMin = newJob.SalaryMin,
            SalaryMax = newJob.SalaryMax,
            ExpiresAt = newJob.ExpiresAt,
            PostedAt = newJob.PostedAt,
            IsActive = newJob.IsActive,
            Company = companyName
        };
    }

    public async Task<JobResponse?> UpdateJobAsync(Guid id, UpdateJobRequest request)
    {
        var job = await _repo.GetByIdAsync(id);
        if (job == null) return null;

        if (job.CompanyId != request.CompanyId)
        {
            throw new UnauthorizedAccessException("This listing can only be updated by the company that owns it.");
        }

        job.Title = request.Title;
        job.Location = request.Location;
        job.Description = request.Description;
        job.Type = request.Type;

        await _repo.UpdateAsync(job);

        return new JobResponse 
        {
            Id = job.Id,
            Title = job.Title,
            Location = job.Location,
            Description = job.Description,
            Type = job.Type,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            ExpiresAt = job.ExpiresAt,
            PostedAt = job.PostedAt,
            IsActive = job.IsActive,
            Company = job.Company?.Name ?? "Unknown"
        };
    }

    public async Task<bool> DeleteJobAsync(Guid id)
    {
        var job = await _repo.GetByIdAsync(id);
        if (job == null) return false;

        await _repo.DeleteAsync(job);
        return true;
    }

    public async Task<IEnumerable<JobResponse>> SearchJobsAsync(string searchTerm)
    {
        var jobs = await _repo.SearchAsync(searchTerm);

        return jobs.Select(j => new JobResponse
        {
            Id = j.Id,
            Title = j.Title,
            Description = j.Description,
            Location = j.Location,
            Type = j.Type,
            PostedAt = j.PostedAt,
            ExpiresAt = j.ExpiresAt,
            CompanyId = j.CompanyId,
        });
    }

    public IAsyncEnumerable<JobListing> GetCompanyJobsCompiledAsync(Guid companyId)
    {
        return _repo.GetListingsByCompanyCompiled(companyId);
    }

    public async Task<IEnumerable<JobListingStatsResponse>> GetCompanyApplicationStatsAsync(Guid companyId)
    {
        return await _repo.GetApplicationStatsAsync(companyId);
    }

    public async Task<PagedResponse<JobResponse>> GetActiveJobsAsync(int page, int pageSize)
    {
        var (items, totalCount) = await _repo.GetActiveListingsPagedAsync(page, pageSize);

        var itemResponses = items.Select(job => new JobResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            Type = job.Type,
            PostedAt = job.PostedAt,
            IsActive = job.IsActive,
            Company = job.Company?.Name ?? "Unknown"
        });

        return new PagedResponse<JobResponse>(itemResponses, totalCount, page, pageSize);
    }

    public async Task<PagedResponse<JobResponse>> GetActiveJobsAsync(JobListingFilterQuery filter)
    {
        var (items, totalCount) = await _repo.GetActiveListingsPagedAsync(filter);

        var itemResponses = items.Select(job => new JobResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            Type = job.Type,
            PostedAt = job.PostedAt,
            IsActive = job.IsActive,
            Company = job.Company?.Name ?? "Unknown"
        });

        int displayPage = filter.Page <= 0 ? 1 : filter.Page;
        int displayPageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        return new PagedResponse<JobResponse>(itemResponses, totalCount, displayPage, displayPageSize);
    }

    public async Task<JobResponse> PatchJobAsync(Guid id, UpdateJobListingRequest request)
    {
        // 1. Get existing listing
        var existingListing = await _repo.GetByIdAsync(id);
        if (existingListing == null)
             throw new JobNotFoundException(id);

        // 2. Validate salary if either salary field is being updated
        if (request.SalaryMin.HasValue || request.SalaryMax.HasValue)
        {
            var newMin = request.SalaryMin ?? existingListing.SalaryMin;
            var newMax = request.SalaryMax ?? existingListing.SalaryMax;
            
            if (newMax < newMin)
                throw new InvalidSalaryException("Salary maximum cannot be less than salary minimum.");
        }

        // 3. Send to repository layer for patching
        var updatedListing = await _repo.PatchAsync(id, request);

        // 4. Map domain model back into your unified JobResponse DTO
        return new JobResponse
        {
            Id = updatedListing.Id,
            Title = updatedListing.Title,
            Description = updatedListing.Description,
            Location = updatedListing.Location,
            Type = updatedListing.Type,
            SalaryMin = updatedListing.SalaryMin,
            SalaryMax = updatedListing.SalaryMax,
            ExpiresAt = updatedListing.ExpiresAt,
            PostedAt = updatedListing.PostedAt,
            IsActive = updatedListing.IsActive,
            Company = updatedListing.Company?.Name ?? "Unknown"
        };
    }
}