namespace FactoryDesignPattern;

/// <summary>
/// CONCRETE PRODUCT — a workspace whose catalog is held as documents.
/// </summary>
public sealed class DocumentProductCatalog : IProductCatalog
{
    public string Backend => "document";

    public Task<Product?> FindAsync(string sku)
    {
        Console.WriteLine($"    [document] products.findOne({{ sku: \"{sku}\" }})");
        return Task.FromResult<Product?>(new Product(sku, "Mechanical Keyboard", 89.00m));
    }
}
