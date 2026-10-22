using AbstractFactoryDesignPattern.Products;

namespace AbstractFactoryDesignPattern.Factories;

/// <summary>
/// ABSTRACT FACTORY — creates a FAMILY of related products for one engine. Two products are enough
/// to see the difference from Factory Method: the client asks for both from the same factory and is
/// guaranteed they belong together.
/// </summary>
/// <remarks>
/// When each product is created through its own separate decision, those decisions are independent
/// and a mixed family is possible. This interface makes the engine one decision for the whole
/// family, so a SQL Server user directory can never be paired with a Postgres role repository.
/// </remarks>
public interface IIdentityStoreFactory
{
    /// <summary>Which engine every product from this factory reads.</summary>
    string Engine { get; }

    IUserDirectory CreateUserDirectory();

    IRoleRepository CreateRoleRepository();
}
