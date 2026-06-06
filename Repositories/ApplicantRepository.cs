using CareerHub.Api.Data;
using CareerHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Repositories
{
    public class ApplicantRepository : IApplicantRepository
    {
        private readonly CareerHubDbContext _context;

        public ApplicantRepository(CareerHubDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Applicant>> GetAllApplicantsAsync()
        {
            return await _context.Applicants
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Applicant?> GetApplicantByIdAsync(Guid id)
        {
            return await _context.Applicants
                .Include(a => a.Applications)
                .ThenInclude(ap => ap.JobListing)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Applicant> CreateApplicantAsync(Applicant applicant)
        {
            await _context.Applicants.AddAsync(applicant);
            await _context.SaveChangesAsync();
            return applicant; 
        }

        public async Task<bool> DoesApplicantExistByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            return await _context.Applicants
                .AnyAsync(a => a.Email != null && a.Email.ToLower() == email.ToLower());
        }
    }
}