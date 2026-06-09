using NSubstitute;
using Xunit;
using CareerHub.Api.Services;
using CareerHub.Api.Repositories;
using CareerHub.Api.Exceptions;
using CareerHub.Api.DTOs;
using CareerHub.Api.Models;

namespace API.Tests.Unit.Services;

public class JobListingServiceTests
{
    private readonly IJobListingRepository _repository;
    private readonly ICompanyRepository _companyRepository;
    private readonly JobService _sut;

    public JobListingServiceTests()
    {
        _repository = Substitute.For<IJobListingRepository>();
        _companyRepository = Substitute.For<ICompanyRepository>();
        _sut = new JobService(_repository);
    }

    [Fact]
    public async Task CreateAsync_WhenSalaryMaxLessThanSalaryMin_ThrowsInvalidSalaryException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var request = new CreateJobRequest
        {
            CompanyId = companyId,
            Title = "Software Engineer",
            SalaryMin = 80000,
            SalaryMax = 50000, // Invalid: Max < Min
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        
        _companyRepository.GetCompanyByIdAsync(companyId).Returns(new Company { Id = companyId });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidSalaryException>(() => _sut.CreateJobAsync(request));
        await _repository.DidNotReceive().AddAsync(Arg.Any<JobListing>());
    }

    [Fact]
    public async Task CreateAsync_WhenExpiresAtIsInThePast_ThrowsInvalidListingException()
    {
        // Arrange
        var request = new CreateJobRequest
        {
            CompanyId = Guid.NewGuid(),
            Title = "Software Engineer",
            SalaryMin = 50000,
            SalaryMax = 80000,
            ExpiresAt = DateTime.UtcNow.AddDays(-1) // Past date
        };
        
        _companyRepository.GetCompanyByIdAsync(request.CompanyId).Returns(new Company());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidListingException>(() => _sut.CreateJobAsync(request));
        await _repository.DidNotReceive().AddAsync(Arg.Any<JobListing>());
    }

    [Fact]
    public async Task CreateAsync_WhenValid_CallsAddAsyncExactlyOnce()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var request = new CreateJobRequest
        {
            CompanyId = companyId,
            Title = "Software Engineer",
            Description = "Great job",
            SalaryMin = 50000,
            SalaryMax = 80000,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        
        _companyRepository.GetCompanyByIdAsync(companyId).Returns(new Company { Id = companyId });

        // Act
        await _sut.CreateJobAsync(request);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<JobListing>());
    }

    [Fact]
    public async Task PatchAsync_WhenOnlySalaryMinChanged_CallsValidation()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var existingListing = new JobListing
        {
            Id = listingId,
            SalaryMin = 50000,
            SalaryMax = 80000,
            Title = "Old Title"
        };
        
        var patchRequest = new UpdateJobListingRequest // Fixed: Changed to UpdateJobListingRequest
        {
            SalaryMin = 90000 // Would exceed existing SalaryMax
        };
        
        _repository.GetByIdAsync(listingId).Returns(existingListing);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidSalaryException>(() => 
            _sut.PatchJobAsync(listingId, patchRequest));
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<JobListing>());
    }

    [Fact]
    public async Task PatchAsync_WhenOnlyTitleChanged_DoesNotCallSalaryValidation()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var existingListing = new JobListing
        {
            Id = listingId,
            SalaryMin = 50000,
            SalaryMax = 80000,
            Title = "Old Title"
        };
        
        var patchRequest = new UpdateJobListingRequest // Fixed: Changed to UpdateJobListingRequest
        {
            Title = "New Title" // Only title changed, no salary fields
        };
        
        _repository.GetByIdAsync(listingId).Returns(existingListing);

        // Act
        await _sut.PatchJobAsync(listingId, patchRequest);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Any<JobListing>());
    }

    [Fact]
    public async Task PatchAsync_WhenListingNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        _repository.GetByIdAsync(listingId).Returns((JobListing)null);

        // Act & Assert
        await Assert.ThrowsAsync<JobNotFoundException>(() => 
            _sut.PatchJobAsync(listingId, Arg.Any<UpdateJobListingRequest>()));
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<JobListing>());
    }
}