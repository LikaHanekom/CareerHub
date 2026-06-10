using System;
namespace CareerHub.Api.DTOs
{
    public class CreateApplicant
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
    }
}