using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareerHub.Api.Data;
using CareerHub.Api.DTOs;
using CareerHub.Api.Models;
using CareerHub.Api.Repositories; 
using CareerHub.Api.Exceptions;
using CareerHub.Api.Enums; 

namespace CareerHub.Api.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepo;
        private readonly IJobListingRepository _jobRepo;


        public ApplicationService(IApplicationRepository applicationRepo, IJobListingRepository jobRepo)
        {
            _applicationRepo = applicationRepo;
            _jobRepo = jobRepo;
            
        }

        public async Task<Application> SubmitApplicationAsync(ApplicationRequest request)
        {
            var isListingOpen = await _jobRepo.IsListingOpenAsync(request.JobListingId);
            if (!isListingOpen)
            {
                throw new ListingClosedException(request.JobListingId);
            }

            var alreadyApplied = await _applicationRepo.HasApplicantAlreadyAppliedAsync(request.ApplicantId, request.JobListingId);
            if (alreadyApplied)
            {
                throw new DuplicateApplicationException(request.ApplicantId);
            }

            var application = new Application
            { 
                JobListingId = request.JobListingId,
                ApplicantId = request.ApplicantId,
                SubmittedAt = DateTime.UtcNow,
                Status = ApplicationStatus.Submitted 
            };

            await _applicationRepo.AddAsync(application);
            return application;
        }

        public async Task UpdateStatusAsync(Guid applicantId, Guid jobListingId, ApplicationStatus newStatus)
        {
            var applications = await _applicationRepo.GetApplicationsForListingAsync(jobListingId);
            var application = applications.FirstOrDefault(a => a.ApplicantId == applicantId);

            if (application == null)
            {
                throw new ApplicationNotFoundException(jobListingId);
            }

            if (!ApplicationStatusValidator.IsValidTransition(application.Status, newStatus))
            {
                throw new InvalidStatusTransitionException(application.Status.ToString(), newStatus.ToString());
            }

            application.Status = newStatus;
            await _applicationRepo.UpdateAsync(application);
        }

        public async Task WithdrawApplicationAsync(Guid applicationId, Guid requestingApplicantId)
        {
            var application = await _applicationRepo.GetApplicationByIdAsync(applicationId);
            if (application == null)
            {
                throw new ApplicationNotFoundException(applicationId);
            }

            if (application.ApplicantId != requestingApplicantId)
            {
                throw new UnauthorizedAccessException("Applicants are only permitted to withdraw their own applications.");
            }

            application.Status = CareerHub.Api.Enums.ApplicationStatus.Rejected;
            await _applicationRepo.UpdateAsync(application);
        }

        public bool IsValidTransition(ApplicationStatus currentStatus, ApplicationStatus targetStatus)
        {
            return ApplicationStatusValidator.IsValidTransition(currentStatus, targetStatus);
        }

        
        public async Task<Application?> PartialUpdateStatusAsync(Guid id, UpdateStatusRequest request)
        {
            // Fetch using existing repository method instead of DbContext
            var app = await _applicationRepo.GetApplicationByIdAsync(id); 
            if (app == null) return null;

            // Guard business state machine transitions explicitly
            if ((app.Status == ApplicationStatus.Rejected || app.Status == ApplicationStatus.Offered) 
                && request.Status == ApplicationStatus.Submitted) 
            {
                throw new ArgumentException("Illegal transition back to Submitted state once finalized."); 
            }

            // Apply changes and save via repository layer
            app.Status = request.Status; 
            await _applicationRepo.UpdateAsync(app); 

            return app;
        }
        public async Task<Application?> GetApplicationByIdAsync(Guid id)
        {
            // Simply fetch the tracked application straight from your repository gate
            return await _applicationRepo.GetApplicationByIdAsync(id);
        }
    }
}