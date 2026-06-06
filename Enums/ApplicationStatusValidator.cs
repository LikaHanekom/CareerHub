using System.Collections.Generic;
using CareerHub.Api.Enums;

namespace CareerHub.Api.Enums;

public static class ApplicationStatusValidator
{
    // 1. The rules are defined in HashSet of value pairs
    private static readonly HashSet<(ApplicationStatus From, ApplicationStatus To)> ValidTransitions = new()
    {
        (ApplicationStatus.Submitted,   ApplicationStatus.UnderReview),
        (ApplicationStatus.UnderReview, ApplicationStatus.Shortlisted),
        (ApplicationStatus.UnderReview, ApplicationStatus.Rejected),
        (ApplicationStatus.Shortlisted, ApplicationStatus.Offered),
        (ApplicationStatus.Shortlisted, ApplicationStatus.Rejected)
    };

    public static bool IsValidTransition(ApplicationStatus currentStatus, ApplicationStatus targetStatus)
    {
        
        return ValidTransitions.Contains((currentStatus, targetStatus));
    }
}