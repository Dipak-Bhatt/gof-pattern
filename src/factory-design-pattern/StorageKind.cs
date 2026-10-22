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
