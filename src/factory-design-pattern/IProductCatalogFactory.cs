namespace FactoryDesignPattern;

/// <summary>
/// CREATOR — declares the factory method. It returns the PRODUCT (<see cref="IProductCatalog"/>),
/// so callers never name a concrete catalog.
/// </summary>
/// <remarks>
/// This is the parameterised form of Factory Method: the workspace's storage kind, not a subclass,
/// chooses the product. The classic subclass form is the same decision moved into one creator type
/// per product (for example <c>RelationalCatalogCreator</c> and <c>DocumentCatalogCreator</c>); the
/// parameterised form is used when the choice is runtime data rather than a compile-time choice.
/// </remarks>
public interface IProductCatalogFactory
{
    IProductCatalog Create(Guid workspaceId);
}
