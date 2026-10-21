namespace ProxyDesignPattern.Protection;

/// <summary>Who is calling. The proxy allows the protected members only for some roles.</summary>
public enum Role
{
    Employee,
    Manager,
    Hr
}
