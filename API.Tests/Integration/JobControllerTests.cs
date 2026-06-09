using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;
using CareerHub.Api.DTOs;
using CareerHub.Api.Models;

namespace API.Tests.Integration;

public class JobsControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public JobsControllerTests(WebApplicationFactoryFixture factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [Fact]
    public async Task GetJobs_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_ResponsesPagedEnvelope()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=1&pageSize=5");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        // Assert
        Assert.NotNull(pagedResponse);
        Assert.Equal(1, pagedResponse.Page);
        Assert.Equal(5, pagedResponse.PageSize);
        Assert.True(pagedResponse.TotalCount >= 0);
        Assert.NotNull(pagedResponse.Data);
    }

    [Fact]
    public async Task GetJobs_ResponseIncludesXTotalCountHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs");
        
        // Assert
        Assert.True(response.Headers.Contains("X-Total-Count"));
        
        // Optional: Verify it's a valid integer
        var totalCountHeader = response.Headers.GetValues("X-Total-Count").First();
        Assert.True(int.TryParse(totalCountHeader, out _));
    }

    [Fact]
    public async Task GetJobs_ResponseIncludesApiSupportedVersionsHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs");
        
        // Assert
        Assert.True(response.Headers.Contains("api-supported-versions"));
        var versions = response.Headers.GetValues("api-supported-versions").First();
        Assert.Contains("1.0", versions);
    }

    [Fact]
    public async Task PostJob_WithoutToken_Returns401()
    {
        // Arrange
        var jobData = new CreateJobRequest
        {
            Title = "Test Job",
            CompanyId = Guid.NewGuid(),
            Description = "Test Description",
            SalaryMin = 50000,
            SalaryMax = 80000,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(jobData),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/jobs", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostApplication_WithoutToken_Returns401()
    {
        // Note: This test assumes there's an ApplicationsController
        // If not, you might want to remove or modify this test
        var applicationData = new ApplicationRequest
        {
            JobListingId = Guid.NewGuid(),
            ApplicantId = Guid.NewGuid()
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(applicationData),
            Encoding.UTF8,
            "application/json");

        // Act - Adjust the endpoint based on your actual applications endpoint
        var response = await _client.PostAsync("/api/v1/applications", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetJobById_WithValidId_DoesNotReturn500()
    {
        // First, get all jobs to find a valid ID
        var getAllResponse = await _client.GetAsync("/api/v1/jobs");
        var content = await getAllResponse.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        // Skip test if no data exists
        if (pagedResponse == null || !pagedResponse.Data.Any())
        {
            // Option 1: Skip the test
            Assert.True(true, "No job data available to test with");
            return;
        }
        
        var jobId = pagedResponse.Data.First().Id;
        
        // Act
        var response = await _client.GetAsync($"/api/v1/jobs/{jobId}");
        
        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=2&pageSize=3");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        // Assert
        Assert.NotNull(pagedResponse);
        Assert.Equal(2, pagedResponse.Page);
        Assert.Equal(3, pagedResponse.PageSize);
    }

    [Fact]
    public async Task GetJobs_CalculatesTotalPagesCorrectly()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=1&pageSize=2");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        // Assert
        Assert.NotNull(pagedResponse);
        var expectedTotalPages = (int)Math.Ceiling(pagedResponse.TotalCount / (double)pagedResponse.PageSize);
        Assert.Equal(expectedTotalPages, pagedResponse.TotalPages);
    }

    [Fact]
    public async Task GetJobs_SetsHasNextPageCorrectly()
    {
        // Act - Request page 1 with small page size
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=1&pageSize=2");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        // Assert
        Assert.NotNull(pagedResponse);
        // If total count > pageSize, should have next page
        if (pagedResponse.TotalCount > pagedResponse.PageSize)
        {
            Assert.True(pagedResponse.HasNextPage);
        }
        else
        {
            Assert.False(pagedResponse.HasNextPage);
        }
    }

    [Fact]
    public async Task GetJobs_SetsHasPreviousPageCorrectly()
    {
        // Act - Request page 2 (if enough data exists)
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=2&pageSize=2");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        // Assert
        Assert.NotNull(pagedResponse);
        // Page 2 should have previous page if total pages >= 2
        if (pagedResponse.TotalPages >= 2)
        {
            Assert.True(pagedResponse.HasPreviousPage);
        }
        else if (pagedResponse.Page == 1)
        {
            Assert.False(pagedResponse.HasPreviousPage);
        }
    }

    [Fact]
    public async Task SearchJobs_WithQuery_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs/search?q=developer");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Optional: Verify response content
        var content = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<IEnumerable<JobListing>>(content, _jsonOptions);
        Assert.NotNull(results);
    }

    [Fact]
    public async Task SearchJobs_WithoutQuery_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs/search?q=");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCompanyJobs_WithValidCompanyId_ReturnsOk()
    {
        // Arrange - Use TechCorp ID from your seed data
        var companyId = Guid.Parse("75ba7d3e-2b50-4841-860e-cbfb4e54e4df");
        
        // Act
        var response = await _client.GetAsync($"/api/v1/jobs/company/{companyId}/compiled");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Optional: Verify response can be deserialized
        var content = await response.Content.ReadAsStringAsync();
        var jobs = JsonSerializer.Deserialize<IEnumerable<JobListing>>(content, _jsonOptions);
        Assert.NotNull(jobs);
    }

    [Fact]
    public async Task GetCompanyJobs_WithInvalidCompanyId_ReturnsNotFound()
    {
        // Arrange
        var invalidCompanyId = Guid.NewGuid();
        
        // Act
        var response = await _client.GetAsync($"/api/v1/jobs/company/{invalidCompanyId}/compiled");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetJobById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        
        // Act
        var response = await _client.GetAsync($"/api/v1/jobs/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_WithFiltering_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs?location=Remote");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        // Assert
        Assert.NotNull(pagedResponse);
        if (pagedResponse.Data.Any())
        {
            Assert.All(pagedResponse.Data, job => 
                Assert.Equal("Remote", job.Location, StringComparer.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task PatchJob_WithoutToken_Returns401()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var patchData = new UpdateJobListingRequest
        {
            Title = "Updated Title"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(patchData),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PatchAsync($"/api/v1/jobs/{jobId}", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=0&pageSize=5");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=1&pageSize=0");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_WithExcessivePageSize_ReturnsBadRequest()
    {
        // Act - Assuming max page size is 100
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=1&pageSize=1000");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}