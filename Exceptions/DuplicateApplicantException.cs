using System;

namespace CareerHub.Api.Exceptions
{
    public class DuplicateApplicantException : Exception
    {
        public DuplicateApplicantException(string email) 
            : base($"An applicant with the email address '{email}' already exists.")
        {
        }
    }
}