using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerHub.Api.Models; // Tells file to look inside the Models folder
using CareerHub.Api.Enums;
using CareerHub.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Services;//file lives in services layer of project

public class JobService(CareerHubDbContext context)
{

    private readonly CareerHubDbContext _context = context;

    //Get all the jobs
    public async Task<IEnumerable<JobListing>> GetAllJobsAsync()
    {
        return await _context.JobListings.ToListAsync();
    }

    //Get the job by ID
    public async Task<JobListing?> GetJobByIdAsync(Guid id)
    {
        return await _context.JobListings.FindAsync(id);
    }

    //Check for duplications
    public async Task<bool> ExistsAsync(string title, Guid companyId)
    {
        return await _context.JobListings.AnyAsync(j => 
            EF.Functions.ILike(j.Title, title) && 
            j.CompanyId == companyId);
    }

    //Create a Job
    public async Task<JobListing> CreateJobAsync(JobListing job)
    {
        
        // Generate a new client-side Guid since ValueGeneratedNever() is used
        job.Id = Guid.NewGuid();
        job.PostedAt = DateTime.UtcNow;
        
        _context.JobListings.Add(job);
        await _context.SaveChangesAsync();
        
        return job;
    }

    //Update Jobs
    public async Task<JobListing?> UpdateJobAsync(Guid id, JobListing updatedJobData)
    {
        var existingJob = await _context.JobListings.FindAsync(id);
        if (existingJob == null) return null;

        // Map editable fields over
        existingJob.Title = updatedJobData.Title;
        existingJob.Description = updatedJobData.Description;
        existingJob.Location = updatedJobData.Location;
        existingJob.Type = updatedJobData.Type;
        existingJob.IsActive = updatedJobData.IsActive;
        
        // Map the foreign key ID 
        existingJob.CompanyId = updatedJobData.CompanyId;

        await _context.SaveChangesAsync();
        return existingJob;
    }

    //Delete Job
    public async Task<bool> DeleteJobAsync(Guid id)
    {
        var job = await _context.JobListings.FindAsync(id);
        if (job == null) return false;

        _context.JobListings.Remove(job);
        await _context.SaveChangesAsync();
        
        return true;
    }
}