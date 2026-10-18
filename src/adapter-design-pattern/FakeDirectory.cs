namespace AdapterDesignPattern;

/// <summary>The adaptee, in memory. It answers SQL and returns whoever the caller is a tenant for.</summary>
public sealed class FakeDirectory : IEmployeeDirectory
{
    private readonly Dictionary<Guid, Dictionary<int, Employee>> _rows = new();

    public void Seed(Guid tenantId, params Employee[] employees)
        => _rows[tenantId] = employees.ToDictionary(e => e.Id);

    public Task<Employee?> FindAsync(Guid tenantId, int id)
    {
        Console.WriteLine($"    [rigo_hr] SELECT * FROM hr.employee WHERE tenant_id = '{tenantId:D}' AND id = {id}");
        _rows.TryGetValue(tenantId, out var tenant);
        return Task.FromResult(tenant is not null && tenant.TryGetValue(id, out var employee) ? employee : null);
    }
}