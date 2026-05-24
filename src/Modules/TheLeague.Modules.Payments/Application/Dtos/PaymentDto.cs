using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Payments.Application.Dtos;

public record PaymentDto(
    Guid Id,
    Guid MemberId,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    PaymentType Type,
    string? ExternalTransactionId,
    string? FailureReason,
    DateTime PaymentDate,
    decimal PlatformFee,
    string? Description);

public record InvoiceDto(
    Guid Id,
    Guid MemberId,
    string InvoiceNumber,
    InvoiceStatus Status,
    DateTime IssueDate,
    DateTime DueDate,
    decimal TotalAmount,
    decimal PaidAmount,
    string? Notes,
    List<InvoiceLineItemDto> LineItems);

public record InvoiceLineItemDto(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    FeeType FeeType);

public record MemberBalanceDto(
    Guid Id,
    Guid MemberId,
    decimal CreditBalance,
    decimal OutstandingBalance,
    DateTime LastUpdated);

public record BalanceTransactionDto(
    Guid Id,
    Guid MemberId,
    string Type,
    decimal Amount,
    string Description,
    Guid? ReferenceId,
    string? ReferenceType,
    DateTime TransactionDate);

public record RefundDto(
    Guid Id,
    Guid PaymentId,
    Guid MemberId,
    decimal Amount,
    RefundStatus Status,
    string? Reason,
    DateTime? ProcessedAt);
