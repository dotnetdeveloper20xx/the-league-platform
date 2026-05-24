namespace TheLeague.Shared.Infrastructure.Exceptions;

public class ValidationException : DomainException
{
    public List<FieldError> Errors { get; }

    public ValidationException(string message, List<FieldError>? errors = null) : base(message, 400)
    {
        Errors = errors ?? new();
    }

    public ValidationException(List<FieldError> errors) : base("One or more validation errors occurred.", 400)
    {
        Errors = errors;
    }
}

public record FieldError(string Field, string Message);
