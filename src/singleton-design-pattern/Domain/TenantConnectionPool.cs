namespace SingletonDesignPattern.Domain;

/// <summary>
/// The classic Singleton: one instance per process. The pool is an expensive,
/// process-wide resource, so it must never be constructed twice.
/// </summary>
public sealed class TenantConnectionPool
{
    // Lazy + ExecutionAndPublication makes the one-instance guarantee thread-safe
    // without a lock on every access.
    private static readonly Lazy<TenantConnectionPool> _instance =
        new(() => new TenantConnectionPool(), LazyThreadSafetyMode.ExecutionAndPublication);

    private int _openConnections;

    // Private constructor: nothing outside the class can call `new`.
    private TenantConnectionPool()
    {
        _openConnections = 0;
    }

    /// <summary>The one instance.</summary>
    public static TenantConnectionPool Instance => _instance.Value;

    /// <summary>Opens a connection on the shared pool. Thread-safe.</summary>
    public int Open()
    {
        return Interlocked.Increment(ref _openConnections);
    }
}
