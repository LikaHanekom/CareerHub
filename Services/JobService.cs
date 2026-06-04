using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerHub.Api.Models; // Tells file to look inside the Models folder
using CareerHub.Api.Enums;
using CareerHub.Api.Data;
using Microsoft.EntityFrameworkCore;
using CareerHub.Api.DTOs;

namespace CareerHub.Api.Services;//file lives in services layer of project

public class JobService(CareerHubDbContext context) : IJobService
{

    private readonly CareerHubDbContext _context = context;

    //Get all the jobs
    public async Task<IEnumerable<JobResponse>> GetAllJobsAsync()
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
                Company = j.Company.Name,
                ApplicationCount = j.Applications.Count() 
            })
            .ToListAsync();
    }

    //Get the job by ID
    public async Task<JobResponse?> GetJobByIdAsync(Guid id)
{
    
    return await _context.JobListings
        .AsNoTracking()
        .Where(j => j.Id == id)
        .Select(j => new JobResponse
        {
            Id = j.Id,
            Title = j.Title,
            Description = j.Description,
            Location = j.Location,
            Type = j.Type,
            PostedAt = j.PostedAt,
            IsActive = j.IsActive,
            Company = j.Company.Name // Map the navigation property safely
        })
        .FirstOrDefaultAsync();
}
    //Check for duplications
    public async Task<bool> ExistsAsync(string title, Guid companyId)
    {
        return await _context.JobListings.AnyAsync(j => j.Title == title && j.CompanyId == companyId);
    }

    //Create a Job
    public async Task<JobResponse> CreateJobAsync(CreateJobRequest request)
    {
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

        _context.JobListings.Add(newJob);
        await _context.SaveChangesAsync();

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

    //Update Jobs
    public async Task<JobResponse?> UpdateJobAsync(Guid id, UpdateJobRequest request)
    {
        var job = await _context.JobListings.FindAsync(id);
        if (job == null) return null;

        job.Title = request.Title;
        job.CompanyId = request.CompanyId;
        job.Location = request.Location;
        job.Description = request.Description;
        job.Type = request.Type;

        await _context.SaveChangesAsync();


        // Centralized mapping helper to map Domain Models safely to public Response Contracts
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