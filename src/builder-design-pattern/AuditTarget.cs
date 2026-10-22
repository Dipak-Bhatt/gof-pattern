namespace BuilderDesignPattern;

/// <summary>
/// The thing the action was performed on. The id is intentionally <see cref="object"/>
/// because different modules identify their rows with int, long, Guid or string.
/// </summary>
public sealed record AuditTarget(object? Id = null, string? Name = null);
