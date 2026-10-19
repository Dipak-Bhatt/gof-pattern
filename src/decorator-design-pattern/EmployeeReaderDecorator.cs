namespace DecoratorDesignPattern;

/// <summary>
/// BASE DECORATOR — holds the wrapped component and forwards every call to it. Concrete
/// decorators derive from here and add behaviour around the call, never changing the contract.
/// </summary>
public abstract class EmployeeReaderDecorator(IEmployeeReader inner) : IEmployeeReader
{
    public IEmployeeReader Inner { get; } = inner;

    public virtual Task<Employee?> ReadAsync(int id) => Inner.ReadAsync(id);
}
