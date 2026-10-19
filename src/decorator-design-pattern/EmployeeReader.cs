namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE COMPONENT — the thing that actually reads. In production this is the repository the
/// service already calls; here it is a fake database read, so the demo is self-contained.
/// </summary>
public sealed class EmployeeReader : IEmployeeReader
{
    public Task<Employee?> ReadAsync(int id)
    {
        Console.WriteLine($"    [db] SELECT * FROM hr.employee WHERE id = {id}");
        var found = id is 42 or 43;
        return Task.FromResult<Employee?>(found ? new Employee(id, id == 42 ? "Ram Bahadur" : "Sita Sharma") : null);
    }
}
