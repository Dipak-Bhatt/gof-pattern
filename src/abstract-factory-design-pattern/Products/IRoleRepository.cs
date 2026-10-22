namespace AbstractFactoryDesignPattern.Products;

/// <summary>
/// ABSTRACT PRODUCT B — the second member of the same family. It has a SQL Server and a Postgres
/// implementation of its own, which is why a family can drift when each product is created by a
/// separate decision.
/// </summary>
public interface IRoleRepository
{
    /// <summary>The engine this repository reads, for diagnostics and the family check.</summary>
    string Engine { get; }

    Task<ApplicationRole?> FindByNameAsync(string name);
}
