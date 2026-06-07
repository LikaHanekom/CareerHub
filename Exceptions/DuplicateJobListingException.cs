namespace CareerHub.Api.Exceptions
{
    public class DuplicateJobListingException : Exception
    {
        
        public DuplicateJobListingException(string title) 
            : base($"A job listing with the title '{title}' already exists in this company.")
        {
        }
    }
}