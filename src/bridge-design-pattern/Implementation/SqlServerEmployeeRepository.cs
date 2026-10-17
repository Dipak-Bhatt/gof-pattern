using BridgeDesignPattern.Abstraction;
using BridgeDesignPattern.Domain;

namespace BridgeDesignPattern.Implementation;

public class SqlServerEmployeeRepository : IEmployeeRepository
{
    public string Engine => "SQL Server (per-tenant)";

    public Task<Employee?> GetByIdAsync(int id)
    {
        Console.WriteLine($"  [SQL Server] SELECT * FROM Employees WHERE EmployeeId = {id}");
        return Task.FromResult<Employee?>(new Employee
        {
            EmployeeId = id,
            Name = "Ram Bahadur",
            Department = "Engineering",
            Salary = 75000
        });
    }

    public Task AddAsync(Employee employee)
    {
        Console.WriteLine($"  [SQL Server] INSERT INTO Employees (Name, Dept) VALUES ('{employee.Name}', '{employee.Department}')");
        return Task.CompletedTask;
    }
}
