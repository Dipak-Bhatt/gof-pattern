using BridgeDesignPattern.Abstraction;
using BridgeDesignPattern.Domain;

namespace BridgeDesignPattern.Implementation;

public class PostgresEmployeeRepository : IEmployeeRepository
{
    public string Engine => "PostgreSQL (shared, RLS)";

    public Task<Employee?> GetByIdAsync(int id)
    {
        Console.WriteLine($"  [PostgreSQL] SELECT * FROM employees WHERE employee_id = {id} AND tenant_id = current_tenant()");
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
        Console.WriteLine($"  [PostgreSQL] INSERT INTO employees (name, department) VALUES ('{employee.Name}', '{employee.Department}')");
        return Task.CompletedTask;
    }
}
