using CareerHub.Api.Models;
using CareerHub.Api.DTOs;
using CareerHub.Api.Repositories; 
using CareerHub.Api.Exceptions;

namespace CareerHub.Api.Services;

public class JobService : IJobService
{
    private readonly IJobListingRepository _repo;

    public JobService(IJobListingRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<JobResponse>> GetAllJobsAsync()
    {
        return await _repo.GetActiveListingsWithCompanyAsync();
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

    // 🚀 FIX: Added ExistsAsync back to implement the interface member cleanly
    public async Task<bool> ExistsAsync(string title, Guid companyId)
    {
        // Delegate the database check entirely to the repository layer
        return await _repo.DoesListingExistAsync(title, companyId);
    }
                                                                                                        
    public async Task<JobResponse> CreateJobAsync(CreateJobRequest request)
    {
        // Use the check right here to enforce your duplicate business rule!
        var duplicate = await _repo.DoesListingExistAsync(request.Title, request.CompanyId);
        if (duplicate) throw new DuplicateJobListingException(request.Title);

        var newJob = new JobListing
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            CompanyId = request.CompanyId,
            Location = request.Location,
            Description = request.Description,
            Type = request.Type,
            PostedAt = DateTime.UtcNow, 
            IsActive = true            
        };

        await _repo.AddAsync(newJob);

        return new JobResponse 
        {
            Id = newJob.Id,
            Title = newJob.Title,
            Location = newJob.Location,
            Description = newJob.Description,
            Type = newJob.Type,
            PostedAt = newJob.PostedAt,
            IsActive = newJob.IsActive
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
            PostedAt = job.PostedAt,
            IsActive = job.IsActive
        };
    }

    public async Task<bool> DeleteJobAsync(Guid id)
    {
        var job = await _repo.GetByIdAsync(id);
        if (job == null) return false;

        await _repo.DeleteAsync(job);
        return true;
    }
}