namespace TheLeague.Shared.Infrastructure.Exceptions;

public class ServiceUnavailableException : DomainException
{
    public ServiceUnavailableException(string message = "Service temporarily unavailable.") : base(message, 503) { }
}
