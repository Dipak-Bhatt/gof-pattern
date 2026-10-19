namespace AdapterDesignPattern;

/// <summary>
/// ADAPTEE — what the database speaks. A tenant row is keyed <c>(tenant_id, id)</c>, so a find
/// needs the tenant handed to it.
/// </summary>
public interface IEmployeeDirectory
{
    Task<Employee?> FindAsync(Guid tenantId, int id);
}