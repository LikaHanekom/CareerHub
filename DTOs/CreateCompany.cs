namespace CareerHub.Api.DTOs
{
    public class CreateCompanyDto
    {
        public required string Name { get; set; }
        public string? Website { get; set; }
    }
}