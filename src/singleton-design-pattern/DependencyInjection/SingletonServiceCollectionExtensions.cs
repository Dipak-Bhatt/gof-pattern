using Microsoft.Extensions.DependencyInjection;
using SingletonDesignPattern.Domain;

namespace SingletonDesignPattern.DependencyInjection;

/// <summary>
/// The modern form: the container owns the single instance and hands the same
/// one to every scope, instead of the class holding a static <c>Instance</c>.
/// </summary>
public static class SingletonServiceCollectionExtensions
{
    /// <summary>
    /// Registers the process-wide <see cref="TenantConnectionPool"/> as a singleton.
    /// </summary>
    public static IServiceCollection AddTenantConnectionPool(this IServiceCollection services)
    {
        services.AddSingleton(TenantConnectionPool.Instance);
        return services;
    }
}
