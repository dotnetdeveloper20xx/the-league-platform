using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Modules.Documents.Application.Events;

public record DocumentUploadedEvent(
    Guid DocumentId,
    Guid ClubId,
    Guid? MemberId,
    string FileName,
    string DocumentType,
    long FileSize
) : IntegrationEvent
{
    public new Guid? TenantId { get; init; } = ClubId;
}
