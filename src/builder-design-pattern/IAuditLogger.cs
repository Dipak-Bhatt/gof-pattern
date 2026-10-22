namespace BuilderDesignPattern;

/// <summary>
/// The receiver. In production this is the audit writer/queue; here it prints the finished
/// event so the demo is self-contained.
/// </summary>
public interface IAuditLogger
{
    void Write(AuditEvent auditEvent);
}
