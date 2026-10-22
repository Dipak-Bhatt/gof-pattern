namespace AbstractFactoryDesignPattern.Products;

/// <summary>
/// ABSTRACT PRODUCT A — one member of the family a tenant gets from a single engine. The client
/// sees only this interface and never learns which database answered.
/// </summary>
public interface IUserDirectory
{
    /// <summary>The engine this directory reads, for diagnostics and the family check.</summary>
    string Engine { get; }

    Task<AppUser?> FindByUserNameAsync(string userName);

    Task<AppUser> CreateAsync(string userName, string fullName);
}
