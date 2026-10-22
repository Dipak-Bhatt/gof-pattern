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
