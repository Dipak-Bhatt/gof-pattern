namespace AbstractFactoryDesignPattern.Products;

/// <summary>
/// CONCRETE PRODUCT A2 — the shared Postgres database once the tenant's state is
/// <c>postgres</c>.
/// </summary>
public sealed class PgUserDirectory : IUserDirectory
{
    public string Engine => "postgres";

    public Task<AppUser?> FindByUserNameAsync(string userName)
    {
        Console.WriteLine($"    [Postgres] SELECT * FROM public.identity_users WHERE normalizedusername = '{userName.ToUpperInvariant()}'");
        return Task.FromResult<AppUser?>(userName.Equals("ram", StringComparison.OrdinalIgnoreCase)
            ? new AppUser("u-1", userName, "Ram Bahadur")
            : null);
    }

    public Task<AppUser> CreateAsync(string userName, string fullName)
    {
        Console.WriteLine($"    [Postgres] INSERT INTO public.identity_users (username, fullname) VALUES ('{userName}', '{fullName}')");
        return Task.FromResult(new AppUser($"u-{userName}", userName, fullName));
    }
}
