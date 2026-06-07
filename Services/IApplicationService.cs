using System;
using System.Threading.Tasks;
using CareerHub.Api.DTOs;
using CareerHub.Api.Models;
using CareerHub.Api.Enums; 

namespace CareerHub.Api.Services
{
    public interface IApplicationService
    {
        Task<Application> SubmitApplicationAsync(ApplicationRequest request);
        
        Task UpdateStatusAsync(Guid applicantId, Guid jobListingId, ApplicationStatus newStatus);
        
        Task WithdrawApplicationAsync(Guid applicationId, Guid requestingApplicantId);
        
        bool IsValidTransition(ApplicationStatus currentStatus, ApplicationStatus targetStatus);
    }
}