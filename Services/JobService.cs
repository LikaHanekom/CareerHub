using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerHub.Api.Models; // Tells file to look inside the Models folder
using CareerHub.Api.Enums;

namespace CareerHub.Api.Services;//file lives in services layer of project

public class JobService
{
    private static readonly List<JobListing> _jobs = new()
    {
        new JobListing 
        {
            Id = 1, 
            Title = "Full Stack Developer", 
            Company = "TechCorp", 
            Location = "Remote", 
            Description = "Build beautiful React and .NET apps.", 
            Type = JobType.FullTime, //using enum value
            PostedAt = DateTime.UtcNow.AddDays(-5), // Seeded historical date
            IsActive = true
        },
        new JobListing 
        { 
            Id = 2, 
            Title = "Backend Engineer", 
            Company = "DataSystems", 
            Location = "Johannesburg", 
            Description = "Optimize heavy-duty API engines.", 
            Type = JobType.Contract,
            PostedAt = DateTime.UtcNow.AddDays(-2),
            IsActive = true 
        },
        new JobListing 
        { 
            Id = 3, 
            Title = "Junior Web Developer", 
            Company = "CreativeAgency", 
            Location = "Cape Town", 
            Description = "Maintain and style frontend components.", 
            Type = JobType.Internship, 
            PostedAt = DateTime.UtcNow,
            IsActive = true 
        } 
    };

    public async Task<IEnumerable<JobListing>> GetAllJobsAsync() //async: allows to run withou blocking servers execution threads
    {
        return await Task.FromResult(_jobs);
    }

    public async Task<JobListing?> GetJobByIdAsync(int id)
    {
        var job = _jobs.FirstOrDefault(j => j.Id == id); //searches through jobs list to find id that mathes id passed into method
        return await Task.FromResult(job);
    }

    public async Task<bool> ExistsAsync(string title, string company)
    {
        // Part 3 duplicate check: case-insensitive check on Title and Company
        var exists = _jobs.Any(j => j.Title.Equals(title, StringComparison.OrdinalIgnoreCase) && 
                                j.Company.Equals(company, StringComparison.OrdinalIgnoreCase));
        return await Task.FromResult(exists);
    }

    public async Task<JobListing> CreateJobAsync(JobListing job)
    {
        // Generate a new sequential integer ID
        job.Id = _jobs.Any() ? _jobs.Max(j => j.Id) + 1 : 1;
        _jobs.Add(job);
        return await Task.FromResult(job);
    }

    public async Task<JobListing?> UpdateJobAsync(int id, JobListing updatedJobData)
    {
        var existingJob = _jobs.FirstOrDefault(j => j.Id == id);
        if (existingJob == null) return null;

        // Fully replace ONLY the editable fields sent by the client DTO
        existingJob.Title = updatedJobData.Title;
        existingJob.Description = updatedJobData.Description;
        existingJob.Company = updatedJobData.Company;
        existingJob.Location = updatedJobData.Location;
        existingJob.Type = updatedJobData.Type;

        // CRITICAL REQUIREMENT: PostedAt and IsActive values from the original record are strictly preserved here!

        return await Task.FromResult(existingJob);
    }

    public async Task<bool> DeleteJobAsync(int id)
    {
        var job = _jobs.FirstOrDefault(j => j.Id == id);
        if (job == null) return false; // ID doesn't exist

        _jobs.Remove(job);
        return await Task.FromResult(true);
    }
}