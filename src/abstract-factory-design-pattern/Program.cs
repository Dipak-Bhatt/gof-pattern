using AbstractFactoryDesignPattern;
using AbstractFactoryDesignPattern.Factories;

var tenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
var tenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");

Console.WriteLine("=== Abstract Factory Pattern — one tenant, one engine, a family of products ===\n");
Console.WriteLine("ABSTRACT FACTORY   IIdentityStoreFactory");
Console.WriteLine("ABSTRACT PRODUCTS  IUserDirectory, IRoleRepository");
Console.WriteLine("CONCRETE FACTORY   SqlServerIdentityStoreFactory -> SqlUserDirectory + SqlRoleRepository");
Console.WriteLine("                   PostgresIdentityStoreFactory  -> PgUserDirectory  + PgRoleRepository");
Console.WriteLine("CLIENT             UserProvisioningClient (depends on the abstract factory only)");
Console.WriteLine("SELECTOR           TenantIdentityStoreProvider (engine -> concrete factory)\n");

var engines = new FakeTenantEngineResolver()
    .Set(tenantA, TenantEngine.SqlServer)
    .Set(tenantB, TenantEngine.Postgres);

var client = new UserProvisioningClient(new TenantIdentityStoreProvider(engines));

Console.WriteLine("--- Same client, two engines: the whole family changes together ---");
await client.ProvisionAsync(tenantA, "ram", "Ram Bahadur", "HR Admin");
await client.ProvisionAsync(tenantB, "sita", "Sita Sharma", "HR Admin");

Console.WriteLine("--- A role outside the family is refused before a user is written ---");
await client.ProvisionAsync(tenantA, "hari", "Hari Thapa", "Chief Astronaut");

Console.WriteLine("--- Move tenant A to Postgres: both products move in one step ---");
engines.Set(tenantA, TenantEngine.Postgres);
await client.ProvisionAsync(tenantA, "gita", "Gita Karki", "HR Admin");

Console.WriteLine("--- A frozen tenant gets no family at all ---");
engines.Set(tenantB, TenantEngine.Frozen);
try
{
    await client.ProvisionAsync(tenantB, "ram", "Ram Bahadur", "HR Admin");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  {ex.Message}\n");
}

Console.WriteLine("=== Key Insight ===");
Console.WriteLine("Factory Method creates ONE product and leaves the choice per product.");
Console.WriteLine("Abstract Factory creates a FAMILY of related products and makes the choice once,");
Console.WriteLine("so the user directory and the role repository can never end up on different engines.");
Console.WriteLine();
Console.WriteLine("The client never names a concrete product or factory. Adding a third engine means");
Console.WriteLine("adding one concrete factory and its two products; UserProvisioningClient stays unchanged.");
