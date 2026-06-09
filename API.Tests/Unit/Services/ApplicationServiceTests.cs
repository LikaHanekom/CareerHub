using NSubstitute;
using Xunit;
using CareerHub.Api.Services;
using CareerHub.Api.Repositories;
using CareerHub.Api.Models;
using CareerHub.Api.Exceptions;
using CareerHub.Api.DTOs;
using CareerHub.Api.Enums;

namespace API.Tests.Unit.Services;

public class ApplicationServiceTests
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobListingRepository _jobListingRepository;
    private readonly ApplicationService _sut;

    public ApplicationServiceTests()
    {
        _applicationRepository = Substitute.For<IApplicationRepository>();
        _jobListingRepository = Substitute.For<IJobListingRepository>();
        _sut = new ApplicationService(_applicationRepository, _jobListingRepository);
    }

    [Theory]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.UnderReview)]
    [InlineData(ApplicationStatus.UnderReview, ApplicationStatus.Shortlisted)]
    [InlineData(ApplicationStatus.UnderReview, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Shortlisted, ApplicationStatus.Offered)]
    [InlineData(ApplicationStatus.Shortlisted, ApplicationStatus.Rejected)]
    public async Task UpdateStatusAsync_WhenTransitionIsLegal_CallsUpdateAsync(
        ApplicationStatus from, ApplicationStatus to)
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var jobListingId = Guid.NewGuid();
        var application = new Application 
        { 
            Id = Guid.NewGuid(),
            ApplicantId = applicantId,
            JobListingId = jobListingId,
            Status = from 
        };
        
        var applications = new List<Application> { application };
        
        _applicationRepository.GetApplicationsForListingAsync(jobListingId)
            .Returns(applications);

        // Act
        await _sut.UpdateStatusAsync(applicantId, jobListingId, to);

        // Assert
        await _applicationRepository.Received(1).UpdateAsync(Arg.Is<Application>(a => a.Status == to));
    }

    [Theory]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.Offered, ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.UnderReview)]
    [InlineData(ApplicationStatus.Offered, ApplicationStatus.Shortlisted)]
    public async Task UpdateStatusAsync_WhenTransitionIsIllegal_ThrowsInvalidStatusTransitionException(
        ApplicationStatus from, ApplicationStatus to)
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var jobListingId = Guid.NewGuid();
        var application = new Application 
        { 
            Id = Guid.NewGuid(),
            ApplicantId = applicantId,
            JobListingId = jobListingId,
            Status = from 
        };
        
        var applications = new List<Application> { application };
        
        _applicationRepository.GetApplicationsForListingAsync(jobListingId)
            .Returns(applications);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() => 
            _sut.UpdateStatusAsync(applicantId, jobListingId, to));
        
        await _applicationRepository.DidNotReceive().UpdateAsync(Arg.Any<Application>());
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenApplicationNotFound_ThrowsApplicationNotFoundException()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var jobListingId = Guid.NewGuid();
        
        _applicationRepository.GetApplicationsForListingAsync(jobListingId)
            .Returns(new List<Application>()); // Empty list - no applications found

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationNotFoundException>(() => 
            _sut.UpdateStatusAsync(applicantId, jobListingId, ApplicationStatus.UnderReview));
        
        await _applicationRepository.DidNotReceive().UpdateAsync(Arg.Any<Application>());
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenListingIsClosed_ThrowsListingClosedException()
    {
        // Arrange
        var request = new ApplicationRequest
        {
            JobListingId = Guid.NewGuid(),
            ApplicantId = Guid.NewGuid()
        };
        
        _jobListingRepository.IsListingOpenAsync(request.JobListingId)
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<ListingClosedException>(() => 
            _sut.SubmitApplicationAsync(request));
        
        await _applicationRepository.DidNotReceive().AddAsync(Arg.Any<Application>());
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenAlreadyApplied_ThrowsDuplicateApplicationException()
    {
        // Arrange
        var request = new ApplicationRequest
        {
            JobListingId = Guid.NewGuid(),
            ApplicantId = Guid.NewGuid()
        };
        
        _jobListingRepository.IsListingOpenAsync(request.JobListingId).Returns(true);
        _applicationRepository.HasApplicantAlreadyAppliedAsync(request.ApplicantId, request.JobListingId)
            .Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateApplicationException>(() => 
            _sut.SubmitApplicationAsync(request));
        
        await _applicationRepository.DidNotReceive().AddAsync(Arg.Any<Application>());
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenValid_CreatesAndReturnsApplication()
    {
        // Arrange
        var request = new ApplicationRequest
        {
            JobListingId = Guid.NewGuid(),
            ApplicantId = Guid.NewGuid()
        };
        
        _jobListingRepository.IsListingOpenAsync(request.JobListingId).Returns(true);
        _applicationRepository.HasApplicantAlreadyAppliedAsync(request.ApplicantId, request.JobListingId)
            .Returns(false);
        
        Application capturedApplication = null;
        await _applicationRepository.AddAsync(Arg.Do<Application>(app => capturedApplication = app));

        // Act
        var result = await _sut.SubmitApplicationAsync(request);

        // Assert
        await _applicationRepository.Received(1).AddAsync(Arg.Any<Application>());
        Assert.Equal(request.JobListingId, result.JobListingId);
        Assert.Equal(request.ApplicantId, result.ApplicantId);
        Assert.Equal(ApplicationStatus.Submitted, result.Status);
        Assert.True(result.SubmittedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task WithdrawApplicationAsync_WhenApplicationNotFound_ThrowsApplicationNotFoundException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var requestingApplicantId = Guid.NewGuid();
        
        _applicationRepository.GetApplicationByIdAsync(applicationId)
            .Returns((Application)null);

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationNotFoundException>(() => 
            _sut.WithdrawApplicationAsync(applicationId, requestingApplicantId));
    }

    [Fact]
    public async Task WithdrawApplicationAsync_WhenUnauthorized_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var requestingApplicantId = Guid.NewGuid();
        var actualApplicantId = Guid.NewGuid(); // Different from requesting
            
        var application = new Application
        {
            Id = applicationId,
            ApplicantId = actualApplicantId,
            Status = ApplicationStatus.Submitted
        };
        
        _applicationRepository.GetApplicationByIdAsync(applicationId).Returns(application);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _sut.WithdrawApplicationAsync(applicationId, requestingApplicantId));
        
        await _applicationRepository.DidNotReceive().UpdateAsync(Arg.Any<Application>());
    }

    [Fact]
    public async Task WithdrawApplicationAsync_WhenValid_SetsStatusToRejected()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();
            
        var application = new Application
        {
            Id = applicationId,
            ApplicantId = applicantId,
            Status = ApplicationStatus.Submitted
        };
        
        _applicationRepository.GetApplicationByIdAsync(applicationId).Returns(application);

        // Act
        await _sut.WithdrawApplicationAsync(applicationId, applicantId);

        // Assert
        await _applicationRepository.Received(1).UpdateAsync(Arg.Is<Application>(a => a.Status == ApplicationStatus.Rejected));
    }
}