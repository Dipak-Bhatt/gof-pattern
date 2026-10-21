namespace ProxyDesignPattern.Routing;

/// <summary>
/// REAL SUBJECT (SQL Server side) — the implementation for a tenant still on the old engine.
/// </summary>
public sealed class SqlServerWidgetRepository : IWidgetRepository
{
    public string Engine => "sqlserver";

    public Task<int> CountAsync(int seed) => Task.FromResult(seed + 1);
}
