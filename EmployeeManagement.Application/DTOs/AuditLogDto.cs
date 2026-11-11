namespace EmployeeManagement.Application.DTOs;

public sealed record AuditLogDto(
    int Id,
    string? ActorId,
    string ActorEmail,
    string Method,
    string Path,
    int StatusCode,
    DateTimeOffset TimestampUtc);
