using ProxyDesignPattern.Protection;
using ProxyDesignPattern.Routing;

// ─────────────────────────────────────────────────────────────────────────────
// Scenario 1 — a ROUTING proxy: one interface, the call sent to the subject the
// current tenant's engine names, decided on every call.
// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("=== Proxy Pattern — Scenario 1: Routing Proxy ===\n");
Console.WriteLine("The client holds ONE interface, IWidgetRepository. The proxy decides on every");
Console.WriteLine("call which implementation answers, from the tenant's engine.\n");

var engine = TenantEngine.SqlServer;
IWidgetRepository repo = RoutedRepositoryProxy<IWidgetRepository>.Create(
    () => engine,
    () => new SqlServerWidgetRepository(),
    () => new PgWidgetRepository());

Console.WriteLine($"--- Tenant is on {engine} ---");
Console.WriteLine($"  engine: {repo.Engine}");
Console.WriteLine($"  count(0): {await repo.CountAsync(0)}   // routed to SQL Server");

engine = TenantEngine.Postgres;
Console.WriteLine($"\n--- Same interface, tenant now on {engine} ---");
Console.WriteLine($"  engine: {repo.Engine}");
Console.WriteLine($"  count(0): {await repo.CountAsync(0)}   // routed to Postgres");

// ─────────────────────────────────────────────────────────────────────────────
// Scenario 2 — a PROTECTION proxy: one interface, the salary refused unless the
// caller's role may see it. The real record never sees the refused call.
// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("\n=== Proxy Pattern — Scenario 2: Protection Proxy ===\n");
Console.WriteLine("The client holds ONE interface, IEmployeeRecord. The proxy lets the open read");
Console.WriteLine("through and refuses salary unless the caller's role may see it.\n");

foreach (var role in new[] { Role.Employee, Role.Manager, Role.Hr })
{
    Console.WriteLine($"--- Caller is a {role} ---");
    var record = new EmployeeRecord(42, "Ram Bahadur", 85000m);
    IEmployeeRecord guarded = new EmployeeRecordProxy(record, Guid.NewGuid(), role);

    Console.WriteLine($"  name: {guarded.GetName()}   // open to everyone");

    try { Console.WriteLine($"  salary: {guarded.GetSalary():0.00}"); }
    catch (UnauthorizedAccessException) { Console.WriteLine("  salary: ACCESS DENIED"); }

    try
    {
        guarded.SetSalary(90000m);
        Console.WriteLine("  salary update: allowed");
    }
    catch (UnauthorizedAccessException) { Console.WriteLine("  salary update: ACCESS DENIED"); }

    Console.WriteLine();
}

// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("=== Key Insight ===");
Console.WriteLine("Both scenarios are the same shape: the client holds the interface and never");
Console.WriteLine("knows which object is behind it. The ROUTING proxy changes where a call goes;");
Console.WriteLine("the PROTECTION proxy decides whether a call is allowed at all.");
