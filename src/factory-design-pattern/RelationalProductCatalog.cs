namespace FactoryDesignPattern;

/// <summary>
/// CONCRETE PRODUCT — a workspace whose catalog is held in relational tables.
/// </summary>
public sealed class RelationalProductCatalog : IProductCatalog
{
    public string Backend => "relational";

    public Task<Product?> FindAsync(string sku)
    {
        Console.WriteLine($"    [relational] SELECT * FROM product WHERE sku = '{sku}'");
        return Task.FromResult<Product?>(new Product(sku, "Mechanical Keyboard", 89.00m));
    }
}
