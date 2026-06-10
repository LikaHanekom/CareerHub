using System;

namespace CareerHub.Api.Exceptions
{
    public class DuplicateCompanyException : Exception{

        public DuplicateCompanyException(string name) 
            : base($"A company with the name '{name}' already exists in the system.")
        {
        }
    }
}