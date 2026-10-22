namespace AbstractFactoryDesignPattern.Products;

/// <summary>
/// CONCRETE PRODUCT B1 — roles in the tenant's SQL Server database.
/// </summary>
public sealed class SqlRoleRepository : IRoleRepository
{
    public string Engine => "sqlserver";

    public Task<ApplicationRole?> FindByNameAsync(string name)
    {
        Console.WriteLine($"    [SQL Server] SELECT * FROM dbo.AspNetRoles WHERE NormalizedName = '{name.ToUpperInvariant()}'");
        return Task.FromResult<ApplicationRole?>(name.Equals("HR Admin", StringComparison.OrdinalIgnoreCase)
            ? new ApplicationRole("r-1", "HR Admin")
            : null);
    }
}
