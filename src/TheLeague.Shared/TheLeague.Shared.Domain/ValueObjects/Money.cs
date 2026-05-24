namespace TheLeague.Shared.Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "GBP");
