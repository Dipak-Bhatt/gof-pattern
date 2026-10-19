namespace FacadeDesignPattern;

public class Employee
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public bool IsResigning { get; set; }
    public bool IsRetiring { get; set; }
    public DateOnly? ResignationDate { get; set; }
    public DateOnly? RetirementDate { get; set; }
    public bool IsAccountActive { get; set; } = true;
    public bool IsInReportingLine { get; set; } = true;
}

public class InMemoryEmployeeDirectory
{
    private readonly Dictionary<int, Employee> _employees = [];

    public void Seed(params Employee[] employees)
    {
        foreach (var employee in employees)
            _employees[employee.Id] = employee;
    }

    public Employee Get(int employeeId) =>
        _employees.TryGetValue(employeeId, out var employee)
            ? employee
            : throw new InvalidOperationException($"Employee {employeeId} does not exist.");
}
