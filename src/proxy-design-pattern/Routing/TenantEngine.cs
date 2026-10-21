namespace ProxyDesignPattern.Routing;

/// <summary>
/// The database engine a tenant's data currently lives on. The router reads this per call and
/// picks the matching implementation — the decision is not fixed at construction.
/// </summary>
public enum TenantEngine
{
    SqlServer,
    Postgres
}
