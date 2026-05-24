using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Infrastructure.Messaging;

public class InProcessIntegrationEventBus : IIntegrationEventBus, IDisposable
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly Channel<(IntegrationEvent Event, Type EventType)> _channel;
    private readonly ILogger<InProcessIntegrationEventBus> _logger;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _processingTask;

    public InProcessIntegrationEventBus(ILogger<InProcessIntegrationEventBus> logger)
    {
        _logger = logger;
        _channel = Channel.CreateUnbounded<(IntegrationEvent, Type)>();
        _processingTask = ProcessEventsAsync(_cts.Token);
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent
    {
        await _channel.Writer.WriteAsync((@event, typeof(T)), ct);
    }

    public void Subscribe<T>(Func<T, CancellationToken, Task> handler) where T : IntegrationEvent
    {
        var handlers = _handlers.GetOrAdd(typeof(T), _ => new List<Delegate>());
        lock (handlers)
        {
            handlers.Add(handler);
        }
    }

    private async Task ProcessEventsAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var (integrationEvent, eventType) in _channel.Reader.ReadAllAsync(ct))
            {
                if (!_handlers.TryGetValue(eventType, out var handlers))
                    continue;

                List<Delegate> snapshot;
                lock (handlers)
                {
                    snapshot = handlers.ToList();
                }

                foreach (var handler in snapshot)
                {
                    await ExecuteWithRetryAsync(handler, integrationEvent, ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
    }

    private async Task ExecuteWithRetryAsync(Delegate handler, IntegrationEvent @event, CancellationToken ct)
    {
        const int maxRetries = 3;
        var delays = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(25) };

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var task = (Task)handler.DynamicInvoke(@event, ct)!;
                await task;
                return;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex) when (attempt < maxRetries - 1)
            {
                _logger.LogWarning(ex,
                    "Integration event handler failed (attempt {Attempt}/{MaxRetries}) for {EventType}",
                    attempt + 1, maxRetries, @event.GetType().Name);
                await Task.Delay(delays[attempt], ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Integration event handler failed permanently for {EventType} (Id: {EventId})",
                    @event.GetType().Name, @event.Id);
                // Dead-letter: log and continue
            }
        }
    }

    public void Dispose()
    {
        _channel.Writer.Complete();
        _cts.Cancel();
        _cts.Dispose();
    }
}
