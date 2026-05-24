namespace TheLeague.Shared.Contracts.Messaging;

public interface IIntegrationEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent;
    void Subscribe<T>(Func<T, CancellationToken, Task> handler) where T : IntegrationEvent;
}
