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
        
        // Verify it's a valid integer
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
    public async Task GetJobs_WithoutVersion_ReturnsSameStatusAsV1()
    {
        // Act
        var noVersionResponse = await _client.GetAsync("/api/jobs");
        var v1Response = await _client.GetAsync("/api/v1/jobs");

        // Assert
        Assert.Equal(noVersionResponse.StatusCode, v1Response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, noVersionResponse.StatusCode);
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
        var applicationData = new ApplicationRequest
        {
            JobListingId = Guid.NewGuid(),
            ApplicantId = Guid.NewGuid()
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(applicationData),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/v1/applications", content);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetJobById_WithValidId_DoesNotReturn500()
    {
        var getAllResponse = await _client.GetAsync("/api/v1/jobs");
        var content = await getAllResponse.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        if (pagedResponse == null || !pagedResponse.Data.Any())
        {
            Assert.True(true, "No job data available to test with");
            return;
        }
        
        var jobId = pagedResponse.Data.First().Id;
        
        var response = await _client.GetAsync($"/api/v1/jobs/{jobId}");
        
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetJobById_ResponseIncludesETagHeader()
    {
        // First, get a job to test with
        var getAllResponse = await _client.GetAsync("/api/v1/jobs");
        var content = await getAllResponse.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        if (pagedResponse == null || !pagedResponse.Data.Any())
        {
            Assert.True(true, "No job data available to test ETag header");
            return;
        }
        
        var jobId = pagedResponse.Data.First().Id;
        
        // Act
        var response = await _client.GetAsync($"/api/v1/jobs/{jobId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("ETag"));
        var etag = response.Headers.GetValues("ETag").FirstOrDefault();
        Assert.NotNull(etag);
        Assert.NotEmpty(etag);
        // ETag should be wrapped in quotes per RFC 7232
        Assert.True(etag.StartsWith("\"") && etag.EndsWith("\""), "ETag should be quoted");
    }

    [Fact]
    public async Task GetJobById_WithMatchingETag_Returns304()
    {
        // First, get a job to test with
        var getAllResponse = await _client.GetAsync("/api/v1/jobs");
        var content = await getAllResponse.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        if (pagedResponse == null || !pagedResponse.Data.Any())
        {
            Assert.True(true, "No job data available to test ETag 304 response");
            return;
        }
        
        var jobId = pagedResponse.Data.First().Id;
        
        // First request to get ETag
        var firstResponse = await _client.GetAsync($"/api/v1/jobs/{jobId}");
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        
        var etag = firstResponse.Headers.GetValues("ETag").FirstOrDefault();
        Assert.NotNull(etag);
        
        // Second request with If-None-Match header
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/jobs/{jobId}");
        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag));
        
        // Act
        var secondResponse = await _client.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotModified, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GetJobById_WithInvalidId_ReturnsNotFound()
    {
        var invalidId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/v1/jobs/{invalidId}");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_WithPagination_ReturnsCorrectPage()
    {
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=2&pageSize=3");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        Assert.NotNull(pagedResponse);
        Assert.Equal(2, pagedResponse.Page);
        Assert.Equal(3, pagedResponse.PageSize);
    }

    [Fact]
    public async Task GetJobs_CalculatesTotalPagesCorrectly()
    {
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=1&pageSize=2");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        Assert.NotNull(pagedResponse);
        var expectedTotalPages = (int)Math.Ceiling(pagedResponse.TotalCount / (double)pagedResponse.PageSize);
        Assert.Equal(expectedTotalPages, pagedResponse.TotalPages);
    }

    [Fact]
    public async Task GetJobs_SetsHasNextPageCorrectly()
    {
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=1&pageSize=2");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        Assert.NotNull(pagedResponse);
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
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=2&pageSize=2");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
        Assert.NotNull(pagedResponse);
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
        var response = await _client.GetAsync("/api/v1/jobs/search?q=developer");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<IEnumerable<JobListing>>(content, _jsonOptions);
        Assert.NotNull(results);
    }

    [Fact]
    public async Task SearchJobs_WithoutQuery_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/jobs/search?q=");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCompanyJobs_WithValidCompanyId_ReturnsOk()
    {
        // Using TechCorp ID from seed data
        var companyId = Guid.Parse("75ba7d3e-2b50-4841-860e-cbfb4e54e4df");
        
        var response = await _client.GetAsync($"/api/v1/jobs/company/{companyId}/compiled");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jobs = JsonSerializer.Deserialize<IEnumerable<JobListing>>(content, _jsonOptions);
        Assert.NotNull(jobs);
    }

    [Fact]
    public async Task GetCompanyJobs_WithInvalidCompanyId_ReturnsNotFound()
    {
        var invalidCompanyId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/v1/jobs/company/{invalidCompanyId}/compiled");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_WithFiltering_ReturnsFilteredResults()
    {
        var response = await _client.GetAsync("/api/v1/jobs?location=Remote");
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<JobResponse>>(content, _jsonOptions);
        
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
        var jobId = Guid.NewGuid();
        var patchData = new UpdateJobListingRequest
        {
            Title = "Updated Title"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(patchData),
            Encoding.UTF8,
            "application/json");
        
        var response = await _client.PatchAsync($"/api/v1/jobs/{jobId}", content);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_WithInvalidPageNumber_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=0&pageSize=5");
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_WithInvalidPageSize_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=1&pageSize=0");
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_WithExcessivePageSize_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/v1/jobs?pageNumber=1&pageSize=1000");
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}