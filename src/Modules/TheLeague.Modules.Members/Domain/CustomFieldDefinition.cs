using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Members.Domain;

public class CustomFieldDefinition : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string FieldType { get; private set; } = string.Empty; // Text, Number, Date, Boolean, Select, MultiSelect, TextArea
    public bool IsRequired { get; private set; }
    public string? Options { get; private set; } // JSON for Select/MultiSelect
    public int DisplayOrder { get; private set; }

    private CustomFieldDefinition() { }

    public static CustomFieldDefinition Create(Guid clubId, string name, string fieldType, bool isRequired, string? options, int displayOrder)
    {
        return new CustomFieldDefinition
        {
            ClubId = clubId,
            Name = name,
            FieldType = fieldType,
            IsRequired = isRequired,
            Options = options,
            DisplayOrder = displayOrder
        };
    }
}
