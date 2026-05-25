using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Facilities.Domain;

public class FacilityBooking : TenantEntity
{
    public Guid FacilityId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateOnly BookingDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int Duration { get; private set; } // minutes: 30-240 in 30-min increments
    public bool IsMember { get; private set; }
    public decimal PricePaid { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Confirmed;
    public string BookingReference { get; private set; } = string.Empty;
    public DateTime BookedAt { get; private set; } = DateTime.UtcNow;

    public Facility Facility { get; private set; } = null!;

    public static FacilityBooking Create(
        Guid clubId,
        Guid facilityId,
        Guid memberId,
        DateOnly bookingDate,
        TimeOnly startTime,
        int duration,
        bool isMember,
        decimal pricePaid)
    {
        var endTime = startTime.AddMinutes(duration);
        return new FacilityBooking
        {
            ClubId = clubId,
            FacilityId = facilityId,
            MemberId = memberId,
            BookingDate = bookingDate,
            StartTime = startTime,
            EndTime = endTime,
            Duration = duration,
            IsMember = isMember,
            PricePaid = pricePaid,
            BookingReference = GenerateBookingReference(),
            BookedAt = DateTime.UtcNow
        };
    }

    public void Cancel()
    {
        Status = BookingStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateBookingReference()
    {
        return $"FB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}
