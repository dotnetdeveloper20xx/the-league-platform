using FluentValidation;
using MediatR;
using TheLeague.Shared.Infrastructure.Exceptions;
using ValidationException = TheLeague.Shared.Infrastructure.Exceptions.ValidationException;
using FieldError = TheLeague.Shared.Infrastructure.Exceptions.FieldError;

namespace TheLeague.Shared.Infrastructure.Behaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Count != 0)
        {
            var errors = failures.Select(f => new FieldError(f.PropertyName, f.ErrorMessage)).ToList();
            throw new ValidationException(errors);
        }

        return await next();
    }
}
