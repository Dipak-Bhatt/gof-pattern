namespace ProxyDesignPattern.Protection;

/// <summary>
/// REAL SUBJECT — the plain record. It knows nothing about who is calling; it just answers.
/// </summary>
public sealed class EmployeeRecord : IEmployeeRecord
{
    private Employee _employee;

    public EmployeeRecord(int id, string name, decimal salary)
        => _employee = new Employee(id, name, salary);

    public string GetName() => _employee.Name;

    public decimal GetSalary() => _employee.Salary;

    public void SetSalary(decimal value) => _employee = _employee with { Salary = value };
}
