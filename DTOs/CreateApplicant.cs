namespace CareerHub.Api.DTOs
{
    public class CreateApplicantDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
    }
}