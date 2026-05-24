using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Contracts.Events;

public record ProgramCompletedEvent(
    Guid ProgramId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;

public record CertificateIssuedEvent(
    Guid CertificateId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;
