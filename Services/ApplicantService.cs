using CareerHub.Api.Data;
using CareerHub.Api.Models;
using CareerHub.Api.DTOs;       
using CareerHub.Api.Exceptions; 
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Services
{
    public class ApplicantService : IApplicantService
    {
        private readonly CareerHubDbContext _context;

        public ApplicantService(CareerHubDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Applicant>> GetAllApplicantsAsync()
        {
            return await _context.Applicants.ToListAsync();
        }

        public async Task<Applicant?> GetApplicantByIdAsync(Guid id)
        {
            // Includes their applications and the related job listings so they can see their dashboard history!
            return await _context.Applicants
                .Include(a => a.Applications)
                .ThenInclude(ap => ap.JobListing)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Applicant> CreateApplicantAsync(CreateApplicantDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException("Email address cannot be empty.");
            }

            // Duplicate Check
            var exists = await _context.Applicants
                .AnyAsync(a => a.Email != null && a.Email.ToLower() == dto.Email.ToLower());

            if (exists)
            {
                throw new DuplicateApplicantException(dto.Email);
            }

            var applicant = new Applicant
            {
                Id = Guid.NewGuid(), 
                FullName = dto.FullName,
                Email = dto.Email,
                Applications = new List<Application>()
            };

            _context.Applicants.Add(applicant);
            await _context.SaveChangesAsync();
            return applicant;
        }
    }
}