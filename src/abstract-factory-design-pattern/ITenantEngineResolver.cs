namespace AbstractFactoryDesignPattern;

/// <summary>The one input the selector reads: the tenant's engine.</summary>
public interface ITenantEngineResolver
{
    TenantEngine Resolve(Guid tenantId);
}

/// <summary>An in-memory resolver, so the demo can move a tenant between engines.</summary>
public sealed class FakeTenantEngineResolver : ITenantEngineResolver
{
    private readonly Dictionary<Guid, TenantEngine> _engines = [];

    public FakeTenantEngineResolver Set(Guid tenantId, TenantEngine engine)
    {
        _engines[tenantId] = engine;
        return this;
    }

    public TenantEngine Resolve(Guid tenantId) =>
        _engines.TryGetValue(tenantId, out var engine) ? engine : TenantEngine.SqlServer;
}
