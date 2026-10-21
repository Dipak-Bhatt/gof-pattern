namespace ProxyDesignPattern.Routing;

/// <summary>
/// SUBJECT — the one interface the caller depends on. Both implementations and the proxy answer
/// to it, so the client cannot tell which object is behind the variable.
/// </summary>
public interface IWidgetRepository
{
    /// <summary>Which engine answered. The demo prints this to prove the routing actually happened.</summary>
    string Engine { get; }

    Task<int> CountAsync(int seed);
}
