using System;

namespace CareerHub.Api.Exceptions
{
    public class ListingClosedException : Exception
    {
        public ListingClosedException(Guid id) 
            : base($"Job listing '{id}' is already closed or its closing date has passed.") { }
    }
}