namespace CareerHub.Api.Exceptions;

public class InvalidListingException : Exception
{
    public InvalidListingException(string message) : base(message) { }
}