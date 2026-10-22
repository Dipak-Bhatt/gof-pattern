using AbstractFactoryDesignPattern.Factories;
using AbstractFactoryDesignPattern.Products;

namespace AbstractFactoryDesignPattern;

/// <summary>
/// CLIENT — it depends on the abstract factory only. It never names <c>SqlUserDirectory</c>,
/// <c>PgUserDirectory</c>, <c>SqlRoleRepository</c> or <c>PgRoleRepository</c>, so adding a third
/// engine does not change this class.
/// </summary>
public sealed class UserProvisioningClient(TenantIdentityStoreProvider stores)
{
    public async Task ProvisionAsync(Guid tenantId, string userName, string fullName, string roleName)
    {
        IIdentityStoreFactory store = stores.For(tenantId);

        // Both products come from the same factory, so they come from the same engine.
        IUserDirectory users = store.CreateUserDirectory();
        IRoleRepository roles = store.CreateRoleRepository();

        if (!string.Equals(users.Engine, roles.Engine, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Mixed identity family: users from {users.Engine}, roles from {roles.Engine}.");

        var role = await roles.FindByNameAsync(roleName);
        if (role is null)
        {
            Console.WriteLine($"    role '{roleName}' does not exist; nothing provisioned\n");
            return;
        }

        var user = await users.FindByUserNameAsync(userName)
            ?? await users.CreateAsync(userName, fullName);

        Console.WriteLine($"    engine family = {store.Engine}; {user.FullName} ({user.UserName}) gets role {role.Name}\n");
    }
}
