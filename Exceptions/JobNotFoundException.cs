namespace CareerHub.Api.Exceptions;

public class JobNotFoundException : Exception
{
    public JobNotFoundException(int id)
        : base($"The job listing with ID {id} was not found.")
    {
    }
}