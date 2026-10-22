namespace BuilderDesignPattern;

/// <summary>
/// Product. Immutable once built, so a consumer cannot change it after construction.
/// </summary>
public sealed record AuditEvent
{
    public required AuditCategory Category { get; init; }
    public required LogAction Action { get; init; }
    public AuditTarget Target { get; init; } = new();
    public string? Message { get; init; }
    public object? Metadata { get; init; }
    public string? BeforeState { get; init; }
    public string? AfterState { get; init; }
    public Guid? TenantId { get; init; }
    public int? EmployeeId { get; init; }
}
