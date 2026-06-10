using System;

namespace CareerHub.Api.Exceptions
{
    public class ApplicantNotFoundException : Exception
    {
        public ApplicantNotFoundException(Guid id) 
            : base($"Applicant with ID '{id}' was not found.")
        {
        }
    }
}