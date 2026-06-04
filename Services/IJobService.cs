using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerHub.Api.Models;

namespace CareerHub.Api.Services;

public interface IJobService
{
    // Get all the jobs
    Task<IEnumerable<JobListing>> GetAllJobsAsync();

    // Get the job by ID
    Task<JobListing?> GetJobByIdAsync(Guid id);

    // Check for duplications
    Task<bool> ExistsAsync(string title, Guid companyId);

    // Create a Job
    Task<JobListing> CreateJobAsync(JobListing job);

    // Update Jobs
    Task<JobListing?> UpdateJobAsync(Guid id, JobListing updatedJobData);

    // Delete Job
    Task<bool> DeleteJobAsync(Guid id);
}