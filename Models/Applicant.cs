using Microsoft.AspNetCore.Mvc.ApplicationParts;
namespace CareerHub.Api.Models;

public class Applicant
{
    public Guid Id {get; set;}
    public string FullName {get;set;} =string.Empty;

    public string Email {get; set;} = string.Empty;
    //initialization to use custom applications entity
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}