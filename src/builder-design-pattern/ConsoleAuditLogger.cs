using System.Text.Json;

namespace BuilderDesignPattern;

public sealed class ConsoleAuditLogger : IAuditLogger
{
    public void Write(AuditEvent auditEvent)
    {
        var target = (auditEvent.Target.Id, auditEvent.Target.Name) switch
        {
            (null, null) => "-",
            (null, { } name) => name,
            ({ } id, null) => id.ToString() ?? "-",
            ({ } id, { } name) => $"{id} ({name})"
        };

        var line = $"[audit] {auditEvent.Action,-10} {auditEvent.Category,-15} target={target}";

        if (auditEvent.EmployeeId is not null)
            line += $" employee={auditEvent.EmployeeId}";

        if (auditEvent.TenantId is not null)
            line += $" tenant={auditEvent.TenantId}";

        if (auditEvent.Message is not null)
            line += $" message=\"{auditEvent.Message}\"";

        Console.WriteLine(line);

        if (auditEvent.Metadata is not null)
            Console.WriteLine($"        metadata={JsonSerializer.Serialize(auditEvent.Metadata)}");

        if (auditEvent.BeforeState is not null || auditEvent.AfterState is not null)
            Console.WriteLine($"        before={auditEvent.BeforeState ?? "-"} after={auditEvent.AfterState ?? "-"}");
    }
}
