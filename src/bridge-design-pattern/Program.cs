using BridgeDesignPattern.Abstraction;
using BridgeDesignPattern.Domain;
using BridgeDesignPattern.Implementation;

Console.WriteLine("=== Bridge Pattern Demo — Multi-Engine Architecture ===\n");
Console.WriteLine("The service (EmployeeService) depends ONLY on IEmployeeRepository.");
Console.WriteLine("The concrete engine is chosen at runtime.\n");

var engines = new Dictionary<DatabaseEngine, IEmployeeRepository>
{
    [DatabaseEngine.SqlServer] = new SqlServerEmployeeRepository(),
    [DatabaseEngine.PostgreSQL] = new PostgresEmployeeRepository()
};

foreach (var (engineType, repo) in engines)
{
    Console.WriteLine($"--- Tenant on {engineType} ---");
    var service = new EmployeeService(repo);

    var emp = await service.GetEmployeeAsync(42);
    Console.WriteLine($"  Result: {emp?.Name}, {emp?.Department}, ${emp?.Salary:N0}\n");

    await service.HireEmployeeAsync(new Employee
    {
        Name = "Sita Sharma",
        Department = "HR",
        Salary = 65000
    });
    Console.WriteLine();
}

Console.WriteLine("=== Key Insight ===");
Console.WriteLine("EmployeeService never knows which database it talks to.");
Console.WriteLine("Swap the IEmployeeRepository implementation -> service code unchanged.");
Console.WriteLine("Abstraction (service) is decoupled from implementation (repository + engine).");
