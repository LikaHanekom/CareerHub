using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerHub.Api.Models;
using CareerHub.Api.DTOs;

namespace CareerHub.Api.Services;

public interface IJobService
{
    // Get all the jobs
    Task<IEnumerable<JobResponse>> GetAllJobsAsync();

    // Get the job by ID
    Task<JobResponse?> GetJobByIdAsync(Guid id);

    // Check for duplications
    Task<bool> ExistsAsync(string title, Guid companyId);

    // Create a Job
    Task<JobResponse> CreateJobAsync(CreateJobRequest request);

    // Update Jobs
    Task<JobResponse?> UpdateJobAsync(Guid id, UpdateJobRequest request);

    // Delete Job
    Task<bool> DeleteJobAsync(Guid id);

    Task<IEnumerable<JobResponse>> SearchJobsAsync(string searchTerm);

    IAsyncEnumerable<JobListing> GetCompanyJobsCompiledAsync(Guid companyId);
    Task<IEnumerable<JobListingStatsResponse>> GetCompanyApplicationStatsAsync(Guid companyId);
    Task<PagedResponse<JobResponse>> GetActiveJobsAsync(int page, int pageSize);
}