# Factory Method Design Pattern

## Definition

The Factory Method pattern **defines one interface for creating an object and lets the creation
site decide which concrete object to build**. The caller asks for the product through the
interface and never names a concrete class, so the choice can change without touching the caller.

The pattern has four roles:

| Role | In this demo |
|------|--------------|
| **Product** | `IProductCatalog` — what the client depends on |
| **Concrete Product** | `RelationalProductCatalog`, `DocumentProductCatalog` |
| **Creator** | `IProductCatalogFactory` — declares `Create(Guid)` |
| **Concrete Creator** | `KindRoutingCatalogFactory` — the factory method's body |

This demo uses the **parameterised** form: the workspace's storage kind, not a subclass, chooses
the product. The classic GoF form moves the same decision into one creator type per product
(`RelationalCatalogCreator`, `DocumentCatalogCreator`). Use the subclass form when the choice is
made at compile time; use the parameterised form when it comes from runtime data.

## Structure

```
          Client
             │ holds the creator + product interfaces
             ▼
   ┌───────────────────────┐        ┌───────────────────────┐
   │ IProductCatalogFactory│        │  IProductCatalog      │
   │  Create(workspaceId)  │───────▶│  FindAsync(sku)       │
   └───────────▲───────────┘        └───────────▲───────────┘
               │                                │
   ┌───────────┴───────────┐        ┌───────────┴───────────┐
   │KindRoutingCatalog     │        │RelationalProduct      │
   │Factory                │        │Catalog                │
   │                       │        ├───────────────────────┤
   │ reads StorageKind ────┼───┐    │DocumentProductCatalog │
   └───────────────────────┘   │    └───────────────────────┘
                               ▼
                     IStorageKindResolver
                     (relational / document / suspended)
```

The client holds `IProductCatalogFactory` and `IProductCatalog`. It never names
`RelationalProductCatalog` or `DocumentProductCatalog`. Swap the resolver's answer and the next
`Create` builds the other product — the client code does not change.

## Project Layout

```
factory-design-pattern/
├── StorageKind.cs                 — the per-call creation condition
├── Product.cs                     — the data both products return
├── IProductCatalog.cs             — PRODUCT interface
├── RelationalProductCatalog.cs    — CONCRETE PRODUCT (relational tables)
├── DocumentProductCatalog.cs      — CONCRETE PRODUCT (document collection)
├── IStorageKindResolver.cs        — the input the factory method reads
├── IProductCatalogFactory.cs      — CREATOR (declares the factory method)
├── KindRoutingCatalogFactory.cs   — CONCRETE CREATOR (the factory method's body)
├── Program.cs                     — composition root / demo
├── factory-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/factory-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```
=== Factory Method Pattern — one catalog interface, two backends ===

PRODUCT          IProductCatalog
CONCRETE PRODUCT RelationalProductCatalog   relational tables
                 DocumentProductCatalog     a document collection
CREATOR          IProductCatalogFactory.Create(Guid workspaceId)
CONCRETE CREATOR KindRoutingCatalogFactory — asks IStorageKindResolver

--- The client holds one interface and names no backend ---
    var catalog = factory.Create(workspaceId);
    var product = await catalog.FindAsync("KB-01");

  workspace A: stored as relational
    [factory] workspace aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa -> Relational -> creating catalog
    [relational] SELECT * FROM product WHERE sku = 'KB-01'
    Backend = relational; product = Mechanical Keyboard (89.00)

  workspace B: stored as documents
    [factory] workspace bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb -> Document   -> creating catalog
    [document] products.findOne({ sku: "KB-01" })
    Backend = document; product = Mechanical Keyboard (89.00)

--- The choice is per call, from the workspace's current storage ---
    (workspace A finishes its migration between two requests)
  workspace A now resolves to document; the caller's code never changed

--- A suspended workspace gets no product at all ---
  Workspace bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb is suspended; no catalog may be created.

=== Key Insight ===
One interface, two implementations, and one method that decides which to build.
The decision lives in the factory, so it is made in one place from the workspace's
storage kind — not scattered across every service that reads a product.
RelationalProductCatalog and DocumentProductCatalog never know about each other,
and the client never knows which one it was handed.
```

## The Code

### StorageKind.cs

```csharp
namespace FactoryDesignPattern;

/// <summary>
/// Where a workspace's catalog lives right now. The factory reads this per call and builds the
/// matching product — the choice is not fixed when the client is constructed.
/// </summary>
public enum StorageKind
{
    Relational,
    Document,
    Suspended
}
```

### Product.cs

```csharp
namespace FactoryDesignPattern;

/// <summary>The plain data both catalogs return. The client sees this, never a backend row.</summary>
public sealed record Product(string Sku, string Name, decimal Price);
```

### IProductCatalog.cs — Product

```csharp
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
```

### RelationalProductCatalog.cs — Concrete Product

```csharp
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
```

### DocumentProductCatalog.cs — Concrete Product

```csharp
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
```

### IStorageKindResolver.cs

```csharp
namespace FactoryDesignPattern;

/// <summary>The one input the factory method reads: how a workspace's catalog is stored.</summary>
public interface IStorageKindResolver
{
    StorageKind Resolve(Guid workspaceId);
}

/// <summary>An in-memory resolver, so the demo can move a workspace between backends.</summary>
public sealed class InMemoryStorageKindResolver : IStorageKindResolver
{
    private readonly Dictionary<Guid, StorageKind> _kinds = [];

    public InMemoryStorageKindResolver Set(Guid workspaceId, StorageKind kind)
    {
        _kinds[workspaceId] = kind;
        return this;
    }

    public StorageKind Resolve(Guid workspaceId) =>
        _kinds.TryGetValue(workspaceId, out var kind) ? kind : StorageKind.Relational;
}
```

### IProductCatalogFactory.cs — Creator

```csharp
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
```

### KindRoutingCatalogFactory.cs — Concrete Creator

```csharp
namespace FactoryDesignPattern;

/// <summary>
/// CONCRETE CREATOR — the body of the factory method. It asks the resolver how the workspace is
/// stored, refuses a workspace that may not be read, and otherwise builds one product and keeps it.
/// </summary>
public sealed class KindRoutingCatalogFactory(IStorageKindResolver resolver) : IProductCatalogFactory
{
    private readonly Dictionary<StorageKind, IProductCatalog> _built = [];

    public IProductCatalog Create(Guid workspaceId)
    {
        var kind = resolver.Resolve(workspaceId);

        // A suspended workspace is mid-migration: neither backend may be touched.
        if (kind == StorageKind.Suspended)
            throw new InvalidOperationException(
                $"Workspace {workspaceId:D} is suspended; no catalog may be created.");

        // Built once per backend, as a real creator would cache within the scope.
        if (_built.TryGetValue(kind, out var existing))
            return existing;

        Console.WriteLine($"    [factory] workspace {workspaceId:D} -> {kind,-10} -> creating catalog");
        IProductCatalog catalog = kind == StorageKind.Document
            ? new DocumentProductCatalog()
            : new RelationalProductCatalog();
        _built[kind] = catalog;
        return catalog;
    }
}
```

### Program.cs — Composition Root

```csharp
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
```

## Why Factory Method?

- **The caller names no concrete class.** `Program.cs` holds `IProductCatalogFactory` and
  `IProductCatalog`; the `new` for a catalog appears in exactly one method.
- **The decision has one home.** Adding the "which storage" rule to the factory keeps it out of
  every service that reads a product.
- **The product can change per call.** The resolver is read on every `Create`, so a workspace that
  finishes its migration is served by the other product on the next request.
- **Construction can refuse.** A suspended workspace throws before either product is built, which
  is a rule of *creation*, not of reading.

## When to Use

- A family of classes implements one interface and the right one depends on runtime state
  (`relational` vs `document`, region, tier, feature flag)
- Construction is non-trivial or must be centralised (configuration, lifetime, caching, guarding)
- You want callers to depend on an abstraction and not on `new`
- The set of concrete products is expected to grow, and the choice should be updated in one place

## Factory Method vs. Related Patterns

- **Simple Factory** is a single class with a `switch`; **Factory Method** puts the method on a
  creator abstraction so it can be varied or overridden.
- **Abstract Factory** creates a *family* of related products; **Factory Method** creates one
  product.
- **Builder** constructs one complex object step by step; **Factory Method** chooses which object
  to construct in one call.
- **Strategy** swaps an algorithm the caller already holds; **Factory Method** decides which
  strategy/product the caller receives.
- **Proxy** wraps an existing object with the same interface; **Factory Method** creates the object
  in the first place. They combine naturally: a routing proxy can be the thing that decides, per
  call, which product to forward to.

## Notes on This Repo

The example is self-contained: no database, no NuGet packages beyond the .NET SDK, and no
environment configuration. It is intended to be read next to the other pattern demos in this
repository and run on its own.
