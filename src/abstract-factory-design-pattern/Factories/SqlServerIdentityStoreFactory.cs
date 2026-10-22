using AbstractFactoryDesignPattern.Products;

namespace AbstractFactoryDesignPattern.Factories;

/// <summary>
/// CONCRETE FACTORY 1 — builds the SQL Server family. Every method can only return a SQL Server
/// product, so a mixed family is impossible by construction.
/// </summary>
public sealed class SqlServerIdentityStoreFactory : IIdentityStoreFactory
{
    public string Engine => "sqlserver";

    public IUserDirectory CreateUserDirectory() => new SqlUserDirectory();

    public IRoleRepository CreateRoleRepository() => new SqlRoleRepository();
}
