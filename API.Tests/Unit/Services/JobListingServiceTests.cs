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
        _sut = new JobService(_repository, _companyRepository);
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
            SalaryMax = 50000,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        
        // FIX: Return a Company? type (nullable)
        _companyRepository.GetCompanyByIdAsync(companyId)
            .Returns(Task.FromResult<Company?>(new Company { Id = companyId, Name = "Test Company" }));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidSalaryException>(() => _sut.CreateJobAsync(request));
        await _repository.DidNotReceive().AddAsync(Arg.Any<JobListing>());
    }

    [Fact]
    public async Task CreateAsync_WhenExpiresAtIsInThePast_ThrowsInvalidListingException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var request = new CreateJobRequest
        {
            CompanyId = companyId,
            Title = "Software Engineer",
            SalaryMin = 50000,
            SalaryMax = 80000,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        
        // Return a Company? type (nullable)
        _companyRepository.GetCompanyByIdAsync(request.CompanyId)
            .Returns(Task.FromResult<Company?>(new Company { Id = companyId, Name = "Test Company" }));

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
        
        //  Return Company? type
        _companyRepository.GetCompanyByIdAsync(companyId)
            .Returns(Task.FromResult<Company?>(new Company { Id = companyId, Name = "Test Company" }));
        
        _repository.DoesListingExistAsync(request.Title, companyId)
            .Returns(Task.FromResult(false));

        // Act
        var result = await _sut.CreateJobAsync(request);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<JobListing>(listing =>
            listing.Title == request.Title &&
            listing.CompanyId == request.CompanyId &&
            listing.SalaryMin == request.SalaryMin &&
            listing.SalaryMax == request.SalaryMax
        ));
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
        
        var patchRequest = new UpdateJobListingRequest
        {
            SalaryMin = 90000
        };
        
        // FIX: Return JobListing? type (nullable)
        _repository.GetByIdAsync(listingId).Returns(Task.FromResult<JobListing?>(existingListing));

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
        
        var patchRequest = new UpdateJobListingRequest
        {
            Title = "New Title"
        };
        
        // FIX: Return JobListing? type
        _repository.GetByIdAsync(listingId).Returns(Task.FromResult<JobListing?>(existingListing));
        
        var updatedListing = new JobListing
        {
            Id = listingId,
            SalaryMin = 50000,
            SalaryMax = 80000,
            Title = "New Title"
        };
        _repository.PatchAsync(listingId, patchRequest).Returns(Task.FromResult(updatedListing));

        // Act
        var result = await _sut.PatchJobAsync(listingId, patchRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
        await _repository.Received(1).PatchAsync(listingId, patchRequest);
    }

    [Fact]
    public async Task PatchAsync_WhenListingNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var patchRequest = new UpdateJobListingRequest
        {
            Title = "New Title"
        };
        
        // FIX: Return null as JobListing? type
        _repository.GetByIdAsync(listingId).Returns(Task.FromResult<JobListing?>(null));

        // Act & Assert
        await Assert.ThrowsAsync<JobNotFoundException>(() => 
            _sut.PatchJobAsync(listingId, patchRequest));
        
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<JobListing>());
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateListing_ThrowsDuplicateJobListingException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var request = new CreateJobRequest
        {
            CompanyId = companyId,
            Title = "Duplicate Job",
            Description = "This job already exists",
            SalaryMin = 50000,
            SalaryMax = 80000,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        
        _companyRepository.GetCompanyByIdAsync(companyId)
            .Returns(Task.FromResult<Company?>(new Company { Id = companyId, Name = "Test Company" }));
        
        _repository.DoesListingExistAsync(request.Title, companyId)
            .Returns(Task.FromResult(true));

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateJobListingException>(() => 
            _sut.CreateJobAsync(request));
        
        await _repository.DidNotReceive().AddAsync(Arg.Any<JobListing>());
    }
}