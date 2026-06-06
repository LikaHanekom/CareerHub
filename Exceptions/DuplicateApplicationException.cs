using System;

namespace CareerHub.Api.Exceptions
{
    public class DuplicateApplicationException : Exception
    {
        public DuplicateApplicationException(Guid applicantId) 
            : base($"Applicant with ID '{applicantId}' has already submitted an application for this job listing.")
        {
        }
    }
}