namespace ProxyDesignPattern.Routing;

/// <summary>
/// REAL SUBJECT (Postgres side) — the implementation for a tenant whose data has moved.
/// </summary>
public sealed class PgWidgetRepository : IWidgetRepository
{
    public string Engine => "postgres";

    public Task<int> CountAsync(int seed) => Task.FromResult(seed + 1000);
}
