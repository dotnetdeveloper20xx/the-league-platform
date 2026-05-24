using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Subscriptions.Domain;

public class AddOn : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Type { get; private set; } = string.Empty; // "sms_pack", "storage", "white_label", "custom_domain"
    public int? Quantity { get; private set; } // e.g., 1000 SMS credits
}
