using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.ValueObjects;

namespace TheLeague.Modules.Members.Domain;

public class Member : AuditableEntity
{
    public Guid? UserId { get; private set; }
    public string MemberNumber { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public Address? Address { get; private set; }
    public EmergencyContact? PrimaryEmergencyContact { get; private set; }
    public EmergencyContact? SecondaryEmergencyContact { get; private set; }
    public MedicalInfo? MedicalInfo { get; private set; }
    public string? ProfilePhotoUrl { get; private set; }
    public string? FacebookUrl { get; private set; }
    public string? TwitterHandle { get; private set; }
    public string? InstagramHandle { get; private set; }
    public string? LinkedInUrl { get; private set; }
    public string? CustomFieldValues { get; private set; }
    public bool MarketingOptIn { get; private set; }
    public bool SmsOptIn { get; private set; }
    public bool EmailOptIn { get; private set; } = true;
    public bool IsFamilyAccount { get; private set; }
    public Guid? PrimaryMemberId { get; private set; }
    public MemberStatus Status { get; private set; }
    public DateTime JoinedDate { get; private set; }
    public DateTime? LastLoginDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? QRCodeData { get; private set; }
    public Guid? ReferredByMemberId { get; private set; }
    public string? ReferralSource { get; private set; }

    private Member() { }

    public static Member Create(Guid clubId, string firstName, string lastName, string email)
    {
        return new Member
        {
            ClubId = clubId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Status = MemberStatus.Pending,
            JoinedDate = DateTime.UtcNow
        };
    }

    public void SetMemberNumber(string memberNumber)
    {
        MemberNumber = memberNumber;
    }

    public void Update(
        string firstName,
        string lastName,
        string email,
        string? phone,
        DateTime? dateOfBirth,
        Gender? gender,
        Address? address,
        EmergencyContact? primaryEmergencyContact,
        EmergencyContact? secondaryEmergencyContact,
        MedicalInfo? medicalInfo,
        string? profilePhotoUrl,
        string? facebookUrl,
        string? twitterHandle,
        string? instagramHandle,
        string? linkedInUrl,
        string? customFieldValues,
        bool marketingOptIn,
        bool smsOptIn,
        bool emailOptIn)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Address = address;
        PrimaryEmergencyContact = primaryEmergencyContact;
        SecondaryEmergencyContact = secondaryEmergencyContact;
        MedicalInfo = medicalInfo;
        ProfilePhotoUrl = profilePhotoUrl;
        FacebookUrl = facebookUrl;
        TwitterHandle = twitterHandle;
        InstagramHandle = instagramHandle;
        LinkedInUrl = linkedInUrl;
        CustomFieldValues = customFieldValues;
        MarketingOptIn = marketingOptIn;
        SmsOptIn = smsOptIn;
        EmailOptIn = emailOptIn;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        EnsureValidTransition(MemberStatus.Active);
        Status = MemberStatus.Active;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        EnsureValidTransition(MemberStatus.Suspended);
        Status = MemberStatus.Suspended;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        EnsureValidTransition(MemberStatus.Cancelled);
        Status = MemberStatus.Cancelled;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        EnsureValidTransition(MemberStatus.Expired);
        Status = MemberStatus.Expired;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        EnsureValidTransition(MemberStatus.Active);
        Status = MemberStatus.Active;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureValidTransition(MemberStatus newStatus)
    {
        var allowed = GetAllowedTransitions(Status);
        if (!allowed.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition from {Status} to {newStatus}. " +
                $"Allowed transitions from {Status}: {string.Join(", ", allowed)}");
        }
    }

    public static IReadOnlyList<MemberStatus> GetAllowedTransitions(MemberStatus currentStatus)
    {
        return currentStatus switch
        {
            MemberStatus.Pending => new[] { MemberStatus.Active },
            MemberStatus.Active => new[] { MemberStatus.Expired, MemberStatus.Suspended, MemberStatus.Cancelled },
            MemberStatus.Suspended => new[] { MemberStatus.Active },
            MemberStatus.Expired => new[] { MemberStatus.Active },
            MemberStatus.Cancelled => Array.Empty<MemberStatus>(),
            _ => Array.Empty<MemberStatus>()
        };
    }
}
