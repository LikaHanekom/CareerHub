using System;

namespace CareerHub.Api.Exceptions
{
    public class ApplicationNotFoundException : Exception
    {
        public ApplicationNotFoundException(Guid id) 
            : base($"Application with ID '{id}' was not found.")
        {
        }
    }
}