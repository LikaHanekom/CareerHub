using System;

namespace CareerHub.Api.DTOs
{
    public class ApplicantResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ResumeUrl { get; set; }
    }
}