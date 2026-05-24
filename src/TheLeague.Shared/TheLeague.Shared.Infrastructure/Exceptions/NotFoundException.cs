namespace TheLeague.Shared.Infrastructure.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message, 404) { }
    public NotFoundException(string entityName, object id) : base($"{entityName} with id '{id}' was not found.", 404) { }
}
