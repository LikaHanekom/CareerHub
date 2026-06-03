using CareerHub.Api.Models;

namespace CareerHub.Api.Models;

public class Company
{
    public Guid Id {get; set;}
    public string Name {get;set;} =string.Empty;

    public string Website {get; set;} = string.Empty;

    public ICollection<JobListing> Joblistings {get; set;} = new List<JobListing>();
}