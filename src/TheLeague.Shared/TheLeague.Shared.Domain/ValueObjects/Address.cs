namespace TheLeague.Shared.Domain.ValueObjects;

public record Address(
    string? Line1,
    string? Line2,
    string? City,
    string? County,
    string? PostCode,
    string? Country);
