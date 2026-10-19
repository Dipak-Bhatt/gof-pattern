namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE DECORATOR — writes a tenant audit line around the read. The tenant is injected, not
/// asked of the caller, exactly as an RLS-bound context carries it.
/// </summary>
public sealed class AuditingEmployeeReaderDecorator(Guid tenantId, IEmployeeReader inner)
    : EmployeeReaderDecorator(inner)
{
    public override Task<Employee?> ReadAsync(int id)
    {
        Console.WriteLine($"  [Audit] tenant={tenantId:D} read id={id}");
        return Inner.ReadAsync(id);
    }
}
