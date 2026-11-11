namespace EmployeeManagement.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }

    public string? ActorId { get; set; }

    public string ActorEmail { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    public DateTimeOffset TimestampUtc { get; set; }
}
