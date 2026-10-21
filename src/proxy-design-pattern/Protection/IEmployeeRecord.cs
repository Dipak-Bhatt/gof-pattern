namespace ProxyDesignPattern.Protection;

/// <summary>
/// SUBJECT — the one interface the caller depends on. The real record and the proxy both answer
/// to it; only the protected members are guarded.
/// </summary>
public interface IEmployeeRecord
{
    /// <summary>Open to every caller — the name is not sensitive.</summary>
    string GetName();

    /// <summary>Protected: only a caller whose role may see salary.</summary>
    decimal GetSalary();

    /// <summary>Protected: only a caller whose role may change salary.</summary>
    void SetSalary(decimal value);
}
