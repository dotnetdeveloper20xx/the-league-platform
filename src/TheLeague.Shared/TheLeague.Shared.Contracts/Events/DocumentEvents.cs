using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Contracts.Events;

public record DocumentUploadedEvent(
    Guid DocumentId,
    Guid ClubId,
    string FileName) : IntegrationEvent;
