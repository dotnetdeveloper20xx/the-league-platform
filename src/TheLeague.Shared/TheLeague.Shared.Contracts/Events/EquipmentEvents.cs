using TheLeague.Shared.Contracts.Messaging;

namespace TheLeague.Shared.Contracts.Events;

public record LoanOverdueEvent(
    Guid LoanId,
    Guid EquipmentId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;
