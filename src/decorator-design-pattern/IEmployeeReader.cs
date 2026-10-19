namespace DecoratorDesignPattern;

/// <summary>
/// COMPONENT — the interface every reader, plain or decorated, answers. The client depends on
/// this alone, so a decorator is indistinguishable from the object it wraps.
/// </summary>
public interface IEmployeeReader
{
    Task<Employee?> ReadAsync(int id);
}
