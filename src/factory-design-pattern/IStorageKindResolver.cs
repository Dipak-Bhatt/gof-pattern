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
