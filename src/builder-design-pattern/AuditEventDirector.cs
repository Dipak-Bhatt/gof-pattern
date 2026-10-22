namespace BuilderDesignPattern;

/// <summary>
/// Director. Starts a build with the action pre-selected, so the client reads the operation
/// it performs rather than the constructor it calls.
/// </summary>
public sealed class AuditEventDirector(IAuditLogger logger)
{
    public AuditEventBuilder Add() => new(logger, LogAction.Added);

    public AuditEventBuilder Update() => new(logger, LogAction.Updated);

    public AuditEventBuilder Delete() => new(logger, LogAction.Deleted);

    public AuditEventBuilder Upload() => new(logger, LogAction.Uploaded);

    public AuditEventBuilder Record() => new(logger);
}
