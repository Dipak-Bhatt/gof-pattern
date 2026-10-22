namespace AdapterDesignPattern;

/// <summary>
/// ADAPTER — implements the target (<see cref="IEmployeeReader"/>) by wrapping the adaptee
/// (<see cref="IEmployeeDirectory"/>), and translates the one thing that does not line up.
/// </summary>
/// <remarks>
/// The client's <c>ReadAsync(42)</c> becomes <c>FindAsync(tenantId, 42)</c>. The tenant comes from
/// here, not from the caller, so a service cannot read another tenant's row by mistake.
/// </remarks>
public sealed class EmployeeReaderAdapter(Guid tenantId, IEmployeeDirectory directory) : IEmployeeReader
{
    public async Task<Employee?> ReadAsync(int id)
    {
        Console.WriteLine($"  [Adapter] ReadAsync({id})  ->  FindAsync({tenantId:D}, {id})");
        return await directory.FindAsync(tenantId, id);
    }
}