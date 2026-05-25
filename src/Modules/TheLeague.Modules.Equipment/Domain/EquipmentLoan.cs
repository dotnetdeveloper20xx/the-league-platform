using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Equipment.Domain;

public class EquipmentLoan : TenantEntity
{
    public Guid EquipmentId { get; private set; }
    public Guid MemberId { get; private set; }
    public LoanStatus Status { get; private set; }
    public DateTime LoanDate { get; private set; }
    public DateTime ExpectedReturnDate { get; private set; }
    public DateTime? ActualReturnDate { get; private set; }
    public decimal Fee { get; private set; }
    public decimal Deposit { get; private set; }
    public string? Notes { get; private set; }

    // Navigation
    public EquipmentItem Equipment { get; private set; } = null!;

    public static EquipmentLoan Create(
        Guid clubId,
        Guid equipmentId,
        Guid memberId,
        DateTime loanDate,
        DateTime expectedReturnDate,
        decimal fee,
        decimal deposit,
        string? notes)
    {
        // Max 90 days from loan date
        var maxReturnDate = loanDate.AddDays(90);
        if (expectedReturnDate > maxReturnDate)
            expectedReturnDate = maxReturnDate;

        return new EquipmentLoan
        {
            ClubId = clubId,
            EquipmentId = equipmentId,
            MemberId = memberId,
            Status = LoanStatus.Active,
            LoanDate = loanDate,
            ExpectedReturnDate = expectedReturnDate,
            Fee = fee,
            Deposit = deposit,
            Notes = notes
        };
    }

    public void MarkReturned(DateTime returnDate)
    {
        Status = LoanStatus.Returned;
        ActualReturnDate = returnDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkReturnedDamaged(DateTime returnDate)
    {
        Status = LoanStatus.ReturnedDamaged;
        ActualReturnDate = returnDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkOverdue()
    {
        Status = LoanStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOverdue(DateTime currentDate)
    {
        return Status == LoanStatus.Active && currentDate > ExpectedReturnDate;
    }

    public bool OverlapsWith(DateTime startDate, DateTime endDate)
    {
        if (Status == LoanStatus.Returned || Status == LoanStatus.ReturnedDamaged || Status == LoanStatus.Lost)
            return false;

        var loanEnd = ActualReturnDate ?? ExpectedReturnDate;
        return LoanDate < endDate && loanEnd > startDate;
    }
}
