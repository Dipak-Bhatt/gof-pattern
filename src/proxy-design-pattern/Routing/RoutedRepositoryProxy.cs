using System.Reflection;

namespace ProxyDesignPattern.Routing;

/// <summary>
/// ROUTING PROXY — the GoF Proxy pattern used to send a call to one of several implementations.
/// <see cref="DispatchProxy"/> synthesises an <typeparamref name="T"/> at runtime; this class
/// supplies the interception: every method call is redirected to the subject the current
/// tenant's engine names, chosen on each call rather than at construction.
/// </summary>
public class RoutedRepositoryProxy<T> : DispatchProxy where T : class
{
    private Func<TenantEngine> _engine = null!;
    private Func<T> _sqlServer = null!;
    private Func<T> _postgres = null!;

    /// <summary>
    /// Builds the proxy. The engine is a function so the decision is re-read on every call; the
    /// implementations are factories so an unused side is never constructed.
    /// </summary>
    public static T Create(Func<TenantEngine> engine, Func<T> sqlServer, Func<T> postgres)
    {
        var proxy = Create<T, RoutedRepositoryProxy<T>>();
        var self = (RoutedRepositoryProxy<T>)(object)proxy;
        self._engine = engine;
        self._sqlServer = sqlServer;
        self._postgres = postgres;
        return proxy;
    }

    /// <summary>Runs for every call on the interface: forward it to the subject the engine names.</summary>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        => targetMethod!.Invoke(
            _engine() == TenantEngine.Postgres ? _postgres() : _sqlServer(), args);
}
