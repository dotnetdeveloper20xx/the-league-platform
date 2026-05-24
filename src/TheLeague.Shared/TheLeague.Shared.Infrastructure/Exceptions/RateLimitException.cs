namespace TheLeague.Shared.Infrastructure.Exceptions;

public class RateLimitException : DomainException
{
    public RateLimitException(string message = "Rate limit exceeded.") : base(message, 429) { }
}
