using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        
        private static readonly HashSet<(ApplicationStatus From, ApplicationStatus To)> AllowedTransitions = new()
        {
            (ApplicationStatus.Submitted, ApplicationStatus.Reviewing),
            (ApplicationStatus.Reviewing, ApplicationStatus.InterviewScheduled),
            (ApplicationStatus.Reviewing, ApplicationStatus.Rejected),
            (ApplicationStatus.InterviewScheduled, ApplicationStatus.Hired),
            (ApplicationStatus.InterviewScheduled, ApplicationStatus.Rejected)
        };

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
                Id = Guid.NewGuid(), 
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

            if (!IsValidTransition(application.Status, newStatus))
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

            application.Status = CareerHub.Api.Enums.ApplicationStatus.Cancelled;
            await _applicationRepo.UpdateAsync(application);
        }

        
        public bool IsValidTransition(ApplicationStatus currentStatus, ApplicationStatus targetStatus)
        {
            return AllowedTransitions.Contains((currentStatus, targetStatus));
        }
    }
}
