using CareerHub.Api.Data;
using CareerHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly CareerHubDbContext _context;

    private static readonly Func<CareerHubDbContext, Guid, Guid, Task<Application?>> _compiledHasAppliedQuery =
    EF.CompileAsyncQuery((CareerHubDbContext context, Guid applicantId, Guid jobListingId) =>
        context.Applications
            .AsNoTracking()
            .FirstOrDefault(a => a.ApplicantId == applicantId && a.JobListingId == jobListingId));

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
            .Include(a => a.JobListing) 
            .ToListAsync();
    }

    public async Task<IEnumerable<Application>> GetApplicationsForListingAsync(Guid jobListingId)
    {
        return await _context.Applications
            .AsNoTracking()
            .Where(a => a.JobListingId == jobListingId)
            .Include(a => a.Applicant)     
            .ToListAsync();
    }

    
    public async Task<Application?> GetApplicationByIdAsync(Guid id)
    {
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    
    public async Task UpdateAsync(Application application)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync();
    }


    public async Task<bool> HasAppliedCompiledAsync(Guid applicantId, Guid jobListingId)
    {
        var application = await _compiledHasAppliedQuery(_context, applicantId, jobListingId);
        return application != null;
    }
}