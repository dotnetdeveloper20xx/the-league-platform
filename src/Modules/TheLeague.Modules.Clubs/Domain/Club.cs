using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Clubs.Domain;

public class Club : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string PrimaryColor { get; private set; } = "#1E40AF";
    public string SecondaryColor { get; private set; } = "#3B82F6";
    public string? AccentColor { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? Address { get; private set; }
    public string? Website { get; private set; }
    public ClubType ClubType { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? RenewalDate { get; private set; }

    // Payment provider config
    public string? PreferredPaymentProvider { get; private set; }
    public string? StripeAccountId { get; private set; }
    public string? PayPalClientId { get; private set; }

    // Email config
    public string? SendGridApiKey { get; private set; }
    public string? FromEmail { get; private set; }
    public string? FromName { get; private set; }

    public static Club Create(string name, string slug, ClubType clubType)
    {
        return new Club
        {
            Name = name,
            Slug = slug.ToLower().Replace(" ", "-"),
            ClubType = clubType
        };
    }

    public void Update(string name, string? description, string? contactEmail, string? contactPhone, string? address, string? website)
    {
        Name = name;
        Description = description;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        Address = address;
        Website = website;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBranding(string primaryColor, string secondaryColor, string? accentColor, string? logoUrl)
    {
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        AccentColor = accentColor;
        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
