namespace ProxyDesignPattern.Protection;

/// <summary>The sensitive record the proxy guards. Salary is the field that is protected.</summary>
public sealed record Employee(int Id, string Name, decimal Salary);
