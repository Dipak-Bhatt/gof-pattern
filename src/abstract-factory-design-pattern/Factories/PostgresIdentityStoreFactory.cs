using AbstractFactoryDesignPattern.Products;

namespace AbstractFactoryDesignPattern.Factories;

/// <summary>
/// CONCRETE FACTORY 2 — builds the Postgres family. Swapping this factory for the SQL Server one
/// swaps every product the client uses, together.
/// </summary>
public sealed class PostgresIdentityStoreFactory : IIdentityStoreFactory
{
    public string Engine => "postgres";

    public IUserDirectory CreateUserDirectory() => new PgUserDirectory();

    public IRoleRepository CreateRoleRepository() => new PgRoleRepository();
}
