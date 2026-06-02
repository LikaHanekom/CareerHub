using System;

namespace CareerHub.Api.Exceptions;

public class JobNotFoundException : Exception
{
    public JobNotFoundException(Guid id) : base($"Job with ID {id} not found.") { }
}