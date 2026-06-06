using CareerHub.Api.Models;

namespace CareerHub.Api.Repositories
{
    public interface IApplicationRepository
    {
        Task<bool> HasApplicantAlreadyAppliedAsync(Guid applicantId, Guid jobListingId);
        Task AddAsync(Application application);

        Task<IEnumerable<Application>> GetApplicationsByApplicantAsync(Guid applicantId);
        Task<IEnumerable<Application>> GetApplicationsForListingAsync(Guid jobListingId);
    }
}