using System;

namespace CareerHub.Api.Exceptions
{
    public class InvalidStatusTransitionException : Exception
    {
        public InvalidStatusTransitionException(string from, string to) 
            : base($"The transition from status '{from}' to '{to}' is completely invalid.") { }
    }
}