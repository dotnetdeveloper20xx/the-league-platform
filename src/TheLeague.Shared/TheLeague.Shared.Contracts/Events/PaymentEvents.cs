using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Shared.Contracts.Events;

public record PaymentCompletedEvent(
    Guid PaymentId,
    Guid MemberId,
    Guid ClubId,
    decimal Amount,
    PaymentType Type) : IntegrationEvent;

public record PaymentFailedEvent(
    Guid PaymentId,
    Guid MemberId,
    Guid ClubId,
    string Reason) : IntegrationEvent;

public record InvoiceOverdueEvent(
    Guid InvoiceId,
    Guid MemberId,
    Guid ClubId) : IntegrationEvent;

public record RefundProcessedEvent(
    Guid RefundId,
    Guid PaymentId,
    Guid MemberId,
    Guid ClubId,
    decimal Amount) : IntegrationEvent;
