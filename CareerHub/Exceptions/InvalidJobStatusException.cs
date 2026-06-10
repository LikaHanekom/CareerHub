namespace CareerHub.Api.Exceptions;
public class InvalidJobStatusException : Exception{
    public InvalidJobStatusException(Guid id) : base($"The job listing with ID {id} is no longer active and cannot be modified.") 
    {
        
    }

}