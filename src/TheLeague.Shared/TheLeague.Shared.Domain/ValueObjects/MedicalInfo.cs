namespace TheLeague.Shared.Domain.ValueObjects;

public record MedicalInfo(
    string? Conditions,
    string? Allergies,
    string? DoctorName,
    string? DoctorPhone,
    string? BloodType);
