using CareerHub.Api.Models; //tells file to look inside the Models folder

namespace CareerHub.Api.Services;//filr lives in services layer of project

public class JobService
{
    private static readonly List<JobListing> _jobs = new()
    {
        new JobListing { Id = 1, Title = "Full Stack Developer", Company = "TechCorp", Location = "Remote", Description = "Build beautiful React and .NET apps.", JobType = "Full-time" },
        new JobListing { Id = 2, Title = "Backend Engineer", Company = "DataSystems", Location = "Johannesburg", Description = "Optimize heavy-duty API engines.", JobType = "Contract" },
        new JobListing { Id = 3, Title = "Junior Web Developer", Company = "CreativeAgency", Location = "Cape Town", Description = "Maintain and style frontend components.", JobType = "Full-time" }  
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
}