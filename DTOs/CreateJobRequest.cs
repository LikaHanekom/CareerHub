using System.ComponentModel.DataAnnotations;
using CareerHub.Api.Enums;

namespace CareerHub.Api.DTOs;

public class CreateJobRequest : IValidatableObject
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 120 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Company is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Company must be between 2 and 80 characters.")]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [MinLength(20, ErrorMessage = "Description must be at least 20 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Job type is required.")]
    [EnumDataType(typeof(JobType), ErrorMessage = "Invalid job type.")]
    public JobType Type { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Minimum salary must be greater than zero.")]
    public decimal? SalaryMin { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Maximum salary must be greater than zero.")]
    public decimal? SalaryMax { get; set; }

    // Custom cross-field validation rule executed automatically by ASP.NET Core
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SalaryMin.HasValue && SalaryMax.HasValue && SalaryMax.Value <= SalaryMin.Value)
        {
            yield return new ValidationResult(
                "Maximum salary must be greater than the minimum salary.",
                new[] { nameof(SalaryMax), nameof(SalaryMin) }
            );
        }
    }
}