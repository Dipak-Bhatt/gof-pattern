namespace AbstractFactoryDesignPattern.Factories;

/// <summary>
/// SELECTOR — a simple factory over abstract factories: the tenant's engine chooses which concrete
/// factory the client receives. The engine is read once for the whole family, not once per product.
/// </summary>
public sealed class TenantIdentityStoreProvider(ITenantEngineResolver engines)
{
    public IIdentityStoreFactory For(Guid tenantId)
    {
        var engine = engines.Resolve(tenantId);

        // A frozen tenant is mid-migration; neither store may be touched.
        if (engine == TenantEngine.Frozen)
            throw new InvalidOperationException(
                $"Tenant {tenantId:D} is frozen: the migrator holds it, so no identity store may be built.");

        Console.WriteLine($"    [Selector] tenant {tenantId:D} -> {engine,-9} -> {nameof(IIdentityStoreFactory)}");

        return engine == TenantEngine.Postgres
            ? new PostgresIdentityStoreFactory()
            : new SqlServerIdentityStoreFactory();
    }
}
