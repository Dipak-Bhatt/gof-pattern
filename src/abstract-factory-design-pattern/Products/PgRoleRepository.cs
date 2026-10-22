namespace AbstractFactoryDesignPattern.Products;

/// <summary>
/// CONCRETE PRODUCT B2 — roles in the shared Postgres database.
/// </summary>
public sealed class PgRoleRepository : IRoleRepository
{
    public string Engine => "postgres";

    public Task<ApplicationRole?> FindByNameAsync(string name)
    {
        Console.WriteLine($"    [Postgres] SELECT * FROM public.identity_role WHERE lower(name) = '{name.ToLowerInvariant()}'");
        return Task.FromResult<ApplicationRole?>(name.Equals("HR Admin", StringComparison.OrdinalIgnoreCase)
            ? new ApplicationRole("r-1", "HR Admin")
            : null);
    }
}
