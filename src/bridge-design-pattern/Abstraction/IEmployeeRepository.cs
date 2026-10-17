using BridgeDesignPattern.Domain;

namespace BridgeDesignPattern.Abstraction;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);
    Task AddAsync(Employee employee);
    string Engine { get; }
}
