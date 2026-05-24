using MediatR;
using Microsoft.Extensions.Logging;

namespace TheLeague.Shared.Infrastructure.Behaviours;

/// <summary>
/// Marker interface for commands that should be wrapped in a transaction.
/// </summary>
public interface ITransactionalRequest { }

public class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;

    public TransactionBehaviour(ILogger<TransactionBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ITransactionalRequest)
            return await next();

        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Beginning transaction for {RequestName}", requestName);

        // Note: Actual transaction management will be handled per-module via their DbContext
        // This behaviour serves as a hook point for cross-cutting transaction concerns
        try
        {
            var response = await next();
            _logger.LogInformation("Transaction committed for {RequestName}", requestName);
            return response;
        }
        catch
        {
            _logger.LogWarning("Transaction rolled back for {RequestName}", requestName);
            throw;
        }
    }
}
