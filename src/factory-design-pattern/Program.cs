using FactoryDesignPattern;

var shopA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
var shopB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

Console.WriteLine("=== Factory Method Pattern — one catalog interface, two backends ===\n");
Console.WriteLine("PRODUCT          IProductCatalog");
Console.WriteLine("CONCRETE PRODUCT RelationalProductCatalog   relational tables");
Console.WriteLine("                 DocumentProductCatalog     a document collection");
Console.WriteLine("CREATOR          IProductCatalogFactory.Create(Guid workspaceId)");
Console.WriteLine("CONCRETE CREATOR KindRoutingCatalogFactory — asks IStorageKindResolver\n");

var kinds = new InMemoryStorageKindResolver()
    .Set(shopA, StorageKind.Relational)
    .Set(shopB, StorageKind.Document);

IProductCatalogFactory factory = new KindRoutingCatalogFactory(kinds);

Console.WriteLine("--- The client holds one interface and names no backend ---");
Console.WriteLine("    var catalog = factory.Create(workspaceId);");
Console.WriteLine("    var product = await catalog.FindAsync(\"KB-01\");\n");

foreach (var (workspaceId, label) in new[]
         {
             (shopA, "workspace A: stored as relational"),
             (shopB, "workspace B: stored as documents")
         })
{
    Console.WriteLine($"  {label}");
    var catalog = factory.Create(workspaceId);
    var product = await catalog.FindAsync("KB-01");
    Console.WriteLine($"    Backend = {catalog.Backend}; product = {product?.Name} ({product?.Price:0.00})\n");
}

Console.WriteLine("--- The choice is per call, from the workspace's current storage ---");
Console.WriteLine("    (workspace A finishes its migration between two requests)");
kinds.Set(shopA, StorageKind.Document);
var moved = factory.Create(shopA);
Console.WriteLine($"  workspace A now resolves to {moved.Backend}; the caller's code never changed\n");

Console.WriteLine("--- A suspended workspace gets no product at all ---");
kinds.Set(shopB, StorageKind.Suspended);
try
{
    factory.Create(shopB);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  {ex.Message}");
}

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("One interface, two implementations, and one method that decides which to build.");
Console.WriteLine("The decision lives in the factory, so it is made in one place from the workspace's");
Console.WriteLine("storage kind — not scattered across every service that reads a product.");
Console.WriteLine("RelationalProductCatalog and DocumentProductCatalog never know about each other,");
Console.WriteLine("and the client never knows which one it was handed.");
