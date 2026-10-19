using AdapterDesignPattern;

var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var otherTenant = Guid.Parse("99999999-9999-9999-9999-999999999999");

var directory = new FakeDirectory();
directory.Seed(tenantId, new Employee(42, "Ram Bahadur"), new Employee(43, "Sita Sharma"));
directory.Seed(otherTenant, new Employee(42, "Someone Else"));

// The client is handed the target. It cannot tell an adapter from a direct implementation —
// that is the point of the pattern.
IEmployeeReader reader = new EmployeeReaderAdapter(tenantId, directory);

Console.WriteLine("TARGET    IEmployeeReader      ReadAsync(int id)          what the client asks for");
Console.WriteLine("ADAPTER   EmployeeReaderAdapter                            what fits them together");
Console.WriteLine("ADAPTEE   IEmployeeDirectory    FindAsync(tenantId, id)   what the database speaks\n");

Console.WriteLine("--- Without an adapter the client would need the tenant too ---");
Console.WriteLine("    var employee = await directory.FindAsync(tenantId, id);   // ...and know it is keyed that way\n");

Console.WriteLine("--- With the adapter the client only knows its own interface ---");
Console.WriteLine($"  {await reader.ReadAsync(42)}");
Console.WriteLine($"  {await reader.ReadAsync(43)}");
Console.WriteLine($"  {await reader.ReadAsync(99) as object ?? "not found"}");

Console.WriteLine("\n--- Same id, another tenant's adapter ---");
IEmployeeReader otherReader = new EmployeeReaderAdapter(otherTenant, directory);
Console.WriteLine($"  {await otherReader.ReadAsync(42)}");

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("Two interfaces that do not fit each other, and one object that fits them together.");
Console.WriteLine("Neither side is edited and neither knows the other exists, so the client never");
Console.WriteLine("carries the adaptee's key shape. Production: PortedRepository<T>, and");
Console.WriteLine("NotifyInboxCompat for the feed the mobile apps read — the same shape, one method here.");