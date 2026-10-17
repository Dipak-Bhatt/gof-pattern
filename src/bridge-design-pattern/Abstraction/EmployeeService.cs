using BridgeDesignPattern.Domain;

namespace BridgeDesignPattern.Abstraction;

public class EmployeeService(IEmployeeRepository repository)
{
    private readonly IEmployeeRepository _repository = repository;

    public async Task<Employee?> GetEmployeeAsync(int id)
    {
        Console.WriteLine($"  [Service] Fetching employee {id} via {_repository.Engine}");
        return await _repository.GetByIdAsync(id);
    }

    public async Task HireEmployeeAsync(Employee employee)
    {
        Console.WriteLine($"  [Service] Hiring {employee.Name} via {_repository.Engine}");
        await _repository.AddAsync(employee);
    }
}
