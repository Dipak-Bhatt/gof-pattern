namespace AbstractFactoryDesignPattern;

/// <summary>
/// The one variant the whole product family is built for: where a tenant's data lives right now.
/// </summary>
public enum TenantEngine
{
    SqlServer,
    Frozen,
    Postgres,
}
