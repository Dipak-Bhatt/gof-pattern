namespace FactoryDesignPattern;

/// <summary>
/// PRODUCT — the one interface the client depends on. It knows a sku and returns a product; it
/// never learns which storage backend answered.
/// </summary>
public interface IProductCatalog
{
    /// <summary>Which backend this catalog reads, shown for diagnostics in the demo.</summary>
    string Backend { get; }

    Task<Product?> FindAsync(string sku);
}
