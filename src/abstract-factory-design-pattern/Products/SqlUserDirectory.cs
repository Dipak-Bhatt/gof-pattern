namespace AbstractFactoryDesignPattern.Products;

/// <summary>
/// CONCRETE PRODUCT A1 — the tenant's own SQL Server database, the source of truth until it is
/// migrated.
/// </summary>
public sealed class SqlUserDirectory : IUserDirectory
{
    public string Engine => "sqlserver";

    public Task<AppUser?> FindByUserNameAsync(string userName)
    {
        Console.WriteLine($"    [SQL Server] SELECT * FROM dbo.AspNetUsers WHERE NormalizedUserName = '{userName.ToUpperInvariant()}'");
        return Task.FromResult<AppUser?>(userName.Equals("ram", StringComparison.OrdinalIgnoreCase)
            ? new AppUser("u-1", userName, "Ram Bahadur")
            : null);
    }

    public Task<AppUser> CreateAsync(string userName, string fullName)
    {
        Console.WriteLine($"    [SQL Server] INSERT INTO dbo.AspNetUsers (UserName, FullName) VALUES ('{userName}', '{fullName}')");
        return Task.FromResult(new AppUser($"u-{userName}", userName, fullName));
    }
}
