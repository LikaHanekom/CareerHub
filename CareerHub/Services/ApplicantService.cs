using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareerHub.Api.DTOs;        
using CareerHub.Api.Models;      
using CareerHub.Api.Repositories; 
using CareerHub.Api.Exceptions;   
//no Microsoft.EntityFrameworkCore.

namespace CareerHub.Api.Services
{
    public class ApplicantService : IApplicantService
    {
        private readonly IApplicantRepository _applicantRepo;

        public ApplicantService(IApplicantRepository applicantRepo)
        {
            _applicantRepo = applicantRepo;
        }

        public async Task<IEnumerable<Applicant>> GetAllApplicantsAsync()
        {
            return await _applicantRepo.GetAllApplicantsAsync();
        }

        public async Task<Applicant?> GetApplicantByIdAsync(Guid id)
        {
            return await _applicantRepo.GetApplicantByIdAsync(id);
        }

        public async Task<Applicant> CreateApplicantAsync(CreateApplicant dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException("Email address cannot be empty.");
            }

            var exists = await _applicantRepo.DoesApplicantExistByEmailAsync(dto.Email);

            if (exists)
            {
                throw new DuplicateApplicantException(dto.Email);
            }

            var applicant = new Applicant
            {
                Id = Guid.NewGuid(), 
                FullName = dto.FullName, 
                Email = dto.Email
            };

            await _applicantRepo.CreateApplicantAsync(applicant);
            
            return applicant;
        }

        public async Task<bool> DoesApplicantExistByEmailAsync(string email)
        {
            return await _applicantRepo.DoesApplicantExistByEmailAsync(email);
        }
    }
}