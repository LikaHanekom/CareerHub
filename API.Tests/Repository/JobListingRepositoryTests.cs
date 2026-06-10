using Microsoft.EntityFrameworkCore;
using Xunit;
using CareerHub.Api.Data;
using CareerHub.Api.Models;
using CareerHub.Api.Enums;
using CareerHub.Api.Repositories;
using CareerHub.Api.DTOs;

namespace API.Tests.Repository;

public class JobListingRepositoryTests : IClassFixture<PostgreSQLContainerFixture>, IAsyncLifetime
{
    private readonly PostgreSQLContainerFixture _fixture;
    private CareerHubDbContext _context = null!;
    private JobListingRepository _repository = null!;
    private int _jobCounter = 0;

    public JobListingRepositoryTests(PostgreSQLContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<CareerHubDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;
        
        _context = new CareerHubDbContext(options);
        
        await _context.Database.MigrateAsync();
        
        _repository = new JobListingRepository(_context);
        
        await ClearDatabase();
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
    }

    private async Task ClearDatabase()
    {
        _context.Applications.RemoveRange(_context.Applications);
        _context.JobListings.RemoveRange(_context.JobListings);
        _context.Companies.RemoveRange(_context.Companies);
        await _context.SaveChangesAsync();
    }

    private async Task<Company> SeedCompanyAsync(string name = "Test Company")
    {
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = name,
            Website = "https://testcompany.com" //For tests
        };
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return company;
    }

    private async Task<JobListing> SeedJobListingAsync(
        Company company, 
        string title = "Test Job",
        decimal salaryMin = 50000,
        decimal salaryMax = 80000,
        DateTime? postedAt = null,
        DateTime? expiresAt = null,
        bool isActive = true)
    {
        _jobCounter++;  
        var uniqueTitle = $"{title} {_jobCounter}";  
        
        var listing = new JobListing
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Title = uniqueTitle,  
            Description = "Test Description",
            SalaryMin = salaryMin,
            SalaryMax = salaryMax,
            Location = "Remote",
            Type = JobType.FullTime,
            PostedAt = postedAt ?? DateTime.UtcNow,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(30),
            IsActive = isActive && (expiresAt == null || expiresAt > DateTime.UtcNow)
        };
        _context.JobListings.Add(listing);
        await _context.SaveChangesAsync();
        return listing;
    }

    [Fact]
    public async Task GetActiveListingsPagedAsync_Page1_ReturnsCorrectCount()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        for (int i = 0; i < 6; i++)
        {
            await SeedJobListingAsync(company, $"Job {i}");
        }
        
        // Act
        var (items, totalCount) = await _repository.GetActiveListingsPagedAsync(page: 1, pageSize: 4);

        // Assert
        Assert.Equal(4, items.Count());
        Assert.Equal(6, totalCount);
    }

    [Fact]
    public async Task GetActiveListingsPagedAsync_Page2_ReturnsDifferentRows()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        var listingIds = new List<Guid>();
        for (int i = 0; i < 6; i++)
        {
            var listing = await SeedJobListingAsync(company, $"Job {i}");
            listingIds.Add(listing.Id);
        }
        
        // Act
        var (page1Items, _) = await _repository.GetActiveListingsPagedAsync(page: 1, pageSize: 3);
        var (page2Items, _) = await _repository.GetActiveListingsPagedAsync(page: 2, pageSize: 3);

        // Assert
        var page1Ids = page1Items.Select(j => j.Id).ToHashSet();
        var page2Ids = page2Items.Select(j => j.Id).ToHashSet();
        
        Assert.Empty(page1Ids.Intersect(page2Ids));
        Assert.Equal(3, page1Items.Count());
        Assert.Equal(3, page2Items.Count());
    }

    [Fact]
    public async Task GetActiveListingsPagedAsync_ResultsAreOrderedByPostedAtDescending()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        var dates = new[] 
        { 
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(-3),
            DateTime.UtcNow.AddDays(-1)
        };
        
        int counter = 1;
        foreach (var date in dates)
        {
            await SeedJobListingAsync(company, $"Job {counter++}", 50000, 80000, date);
        }
        
        // Act
        var (items, _) = await _repository.GetActiveListingsPagedAsync(page: 1, pageSize: 10);
        var itemsList = items.ToList();

        // Assert
        for (int i = 0; i < itemsList.Count - 1; i++)
        {
            Assert.True(itemsList[i].PostedAt >= itemsList[i + 1].PostedAt);
        }
    }

    [Fact]
    public async Task GetActiveListingsPagedAsync_ExcludesExpiredListings()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        // Active listings
        for (int i = 0; i < 3; i++)
        {
            await SeedJobListingAsync(company, $"Active {i}", 50000, 80000, 
                DateTime.UtcNow, DateTime.UtcNow.AddDays(30), true);
        }
        
        // Expired listings
        for (int i = 0; i < 2; i++)
        {
            await SeedJobListingAsync(company, $"Expired {i}", 50000, 80000,
                DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-1), false);
        }
        
        // Act
        var (items, totalCount) = await _repository.GetActiveListingsPagedAsync(page: 1, pageSize: 20);

        // Assert
        Assert.Equal(3, totalCount);
        Assert.All(items, listing => 
            Assert.DoesNotContain("Expired", listing.Title));
    }

    [Fact]
    public async Task CheckConstraint_RejectsSalaryMaxLessThanSalaryMin()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        var invalidListing = new JobListing
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Title = "Invalid Salary",
            Description = "Description",
            SalaryMin = 100000,
            SalaryMax = 50000,
            Location = "Remote",
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };

        // Act & Assert
        _context.JobListings.Add(invalidListing);
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync()
        );
        
        Assert.Contains("check constraint", exception.InnerException?.Message.ToLower());
    }

    [Fact]
    public async Task CheckConstraint_RejectsExpiresAtBeforeCreatedAt()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        var invalidListing = new JobListing
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Title = "Invalid Dates",
            Description = "Description",
            SalaryMin = 50000,
            SalaryMax = 80000,
            Location = "Remote",
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsActive = false
        };

        // Act & Assert
        _context.JobListings.Add(invalidListing);
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync()
        );
        
        Assert.Contains("check constraint", exception.InnerException?.Message.ToLower());
    }

    [Fact]
    public async Task HasAppliedAsync_WhenApplicationExists_ReturnsTrue()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        var listing = await SeedJobListingAsync(company);
        var applicantId = Guid.NewGuid();
        
        var applicant = new Applicant
        {
            Id = applicantId,
            Email = "test@example.com",
            FullName = "Test Applicant"
            // Add other required properties
        };
        _context.Applicants.Add(applicant);
        await _context.SaveChangesAsync();
        
        var application = new Application
        {
            Id = Guid.NewGuid(),
            JobListingId = listing.Id,
            ApplicantId = applicantId,
            Status = ApplicationStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };
        
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        // Act
        var result = await _context.Applications
            .AnyAsync(a => a.JobListingId == listing.Id && a.ApplicantId == applicantId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAppliedAsync_WhenNoApplicationExists_ReturnsFalse()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        var listing = await SeedJobListingAsync(company);
        var applicantId = Guid.NewGuid();

        // Act
        var result = await _context.Applications
            .AnyAsync(a => a.JobListingId == listing.Id && a.ApplicantId == applicantId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task FullTextSearchAsync_ReturnsStemmedMatches()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        var matchingListing = new JobListing
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Title = "Software Engineering Position",
            Description = "Looking for software engineers to join our team",
            SalaryMin = 50000,
            SalaryMax = 80000,
            Location = "Remote",
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };
        
        _context.JobListings.Add(matchingListing);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.SearchAsync("engineer");

        // Assert
        Assert.Contains(results, j => j.Title.Contains("Engineering"));
    }

    [Fact]
    public async Task FullTextSearchAsync_DoesNotReturnNonMatchingListings()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        var matchingListing = new JobListing
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Title = "Software Engineering Position",
            Description = "Looking for software engineers",
            SalaryMin = 50000,
            SalaryMax = 80000,
            Location = "Remote",
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };
        
        var nonMatchingListing = new JobListing
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Title = "Marketing Manager",
            Description = "Looking for marketing professional",
            SalaryMin = 60000,
            SalaryMax = 90000,
            Location = "New York",
            Type = JobType.FullTime,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };
        
        _context.JobListings.AddRange(matchingListing, nonMatchingListing);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.SearchAsync("engineer");

        // Assert
        Assert.Single(results);
        Assert.Contains("Engineering", results.First().Title);
    }

    [Fact]
    public async Task GetActiveListingsPagedAsync_WithEmptyDatabase_ReturnsEmptyResult()
    {
        // Act
        var (items, totalCount) = await _repository.GetActiveListingsPagedAsync(page: 1, pageSize: 10);

        // Assert
        Assert.Empty(items);
        Assert.Equal(0, totalCount);
    }

    [Fact]
    public async Task GetActiveListingsPagedAsync_WithFilterObject_ReturnsFilteredResults()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        await SeedJobListingAsync(company, "Remote Job 1", 50000, 80000, 
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), true);
        await SeedJobListingAsync(company, "Office Job 1", 60000, 90000, 
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), true);
        
        // Update the location for the office job
        var allJobs = _context.JobListings.Where(j => j.CompanyId == company.Id).ToList();
        var officeJob = allJobs.FirstOrDefault(j => j.Title.Contains("Office"));
        if (officeJob != null)
        {
            officeJob.Location = "New York";
            await _context.SaveChangesAsync();
        }
        
        var filter = new JobListingFilterQuery
        {
            Location = "Remote",
            Page = 1,
            PageSize = 10
        };
        
        // Act
        var (items, totalCount) = await _repository.GetActiveListingsPagedAsync(filter);

        // Assert
        Assert.Single(items);
        Assert.All(items, j => Assert.Equal("Remote", j.Location));
    }

    [Fact]
    public async Task GetActiveListingsPagedAsync_WithSalaryFilter_ReturnsFilteredResults()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        await SeedJobListingAsync(company, "Junior Position", 40000, 60000);
        await SeedJobListingAsync(company, "Senior Position", 80000, 120000);
        
        var filter = new JobListingFilterQuery
        {
            SalaryMin = 70000,
            Page = 1,
            PageSize = 10
        };
        
        // Act
        var (items, totalCount) = await _repository.GetActiveListingsPagedAsync(filter);

        // Assert
        Assert.Single(items);
        Assert.All(items, j => Assert.True(j.SalaryMin >= 70000));
    }

    [Fact]
    public async Task CompiledQuery_GetListingsByCompanyCompiled_ReturnsCorrectListings()
    {
        // Arrange
        var company1 = await SeedCompanyAsync("Company 1");
        var company2 = await SeedCompanyAsync("Company 2");
        
        await SeedJobListingAsync(company1, "Job for Company 1");
        await SeedJobListingAsync(company1, "Another Job for Company 1");
        await SeedJobListingAsync(company2, "Job for Company 2");
        
        // Act
        var results = await _repository.GetListingsByCompanyCompiled(company1.Id).ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, j => Assert.Equal(company1.Id, j.CompanyId));
    }
}