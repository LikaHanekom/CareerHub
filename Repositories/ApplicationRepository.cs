using CareerHub.Api.Data;
using CareerHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly CareerHubDbContext _context;

    public ApplicationRepository(CareerHubDbContext context) => _context = context;

    public async Task<bool> HasApplicantAlreadyAppliedAsync(Guid applicantId, Guid jobListingId)
    {
        return await _context.Applications
        .AnyAsync(a => a.ApplicantId == applicantId && a.JobListingId == jobListingId);
    }

    public async Task AddAsync(Application application)
    {
        await _context.Applications.AddAsync(application);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Application>> GetApplicationsByApplicantAsync(Guid applicantId)
    {
        return await _context.Applications
            .AsNoTracking()
            .Where(a => a.ApplicantId == applicantId)
            .Include(a => a.JobListing) // Job details so they know what they applied for
            .ToListAsync();
    }

   
    public async Task<IEnumerable<Application>> GetApplicationsForListingAsync(Guid jobListingId)
    {
        return await _context.Applications
            .AsNoTracking()
            .Where(a => a.JobListingId == jobListingId)
            .Include(a => a.Applicant) // Applicant profile details     
            .ToListAsync();
    }


}