using System;

namespace CareerHub.Api.DTOs;

public record JobListingStatsResponse(
    Guid JobId,
    string Title,
    int SubmittedCount,
    int UnderReviewCount,
    int ShortlistedCount,
    int RejectedCount,
    int OfferedCount,
    int TotalApplications,
    long Rank 
);