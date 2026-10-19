# Adapter Design Pattern

## Definition

The Adapter pattern **lets two incompatible interfaces work together** by wrapping one of them. The client keeps calling the interface it already knows, and the adapter translates every call into the interface the other side actually speaks. Neither side is edited, and neither knows the other exists.

## Structure

```
┌─────────────────────┐         ┌─────────────────────────┐
│   Client            │────────>│   Target (iface)        │
│                     │  calls   │   IEmployeeReader       │
└─────────────────────┘         └───────────┬─────────────┘
                                            │ implements
                                ┌─────────────────────────┐
                                │   Adapter               │
                                │   EmployeeReaderAdapter │──wraps──┐
                                └─────────────────────────┘          │
                                                                     ▼
                                                       ┌─────────────────────────┐
                                                       │   Adaptee (iface)       │
                                                       │   IEmployeeDirectory    │
                                                       └───────────┬─────────────┘
                                                                   │ implements
                                                       ┌─────────────────────────┐
                                                       │   FakeDirectory         │
                                                       └─────────────────────────┘
```

## Project Layout

```
adapter-design-pattern/
├── Employee.cs                    — shared record
├── IEmployeeReader.cs             — TARGET interface (what the client asks for)
├── IEmployeeDirectory.cs          — ADAPTEE interface (what the other side speaks)
├── EmployeeReaderAdapter.cs       — ADAPTER (implements target, wraps adaptee)
├── FakeDirectory.cs               — adaptee, in memory
├── Program.cs                     — composition root / wiring
└── README.md
```

## How to Run

```bash
dotnet run
```

## Output

```
TARGET    IEmployeeReader      ReadAsync(int id)          what the client asks for
ADAPTER   EmployeeReaderAdapter                            what fits them together
ADAPTEE   IEmployeeDirectory    FindAsync(tenantId, id)   what rigo_hr speaks

--- Without an adapter the client would need the tenant too ---
    var employee = await directory.FindAsync(tenantId, id);   // ...and know it is keyed that way

--- With the adapter the client only knows its own interface ---
  [Adapter] ReadAsync(42)  ->  FindAsync(11111111-1111-1111-1111-111111111111, 42)
    [rigo_hr] SELECT * FROM hr.employee WHERE tenant_id = '11111111-1111-1111-1111-111111111111' AND id = 42
  Employee { Id = 42, Name = Ram Bahadur }
  [Adapter] ReadAsync(43)  ->  FindAsync(11111111-1111-1111-1111-111111111111, 43)
    [rigo_hr] SELECT * FROM hr.employee WHERE tenant_id = '11111111-1111-1111-1111-111111111111' AND id = 43
  Employee { Id = 43, Name = Sita Sharma }
  [Adapter] ReadAsync(99)  ->  FindAsync(11111111-1111-1111-1111-111111111111, 99)
    [rigo_hr] SELECT * FROM hr.employee WHERE tenant_id = '11111111-1111-1111-1111-111111111111' AND id = 99
  not found

--- Same id, another tenant's adapter ---
  [Adapter] ReadAsync(42)  ->  FindAsync(99999999-9999-9999-9999-999999999999, 42)
    [rigo_hr] SELECT * FROM hr.employee WHERE tenant_id = '99999999-9999-9999-9999-999999999999' AND id = 42
  Employee { Id = 42, Name = Someone Else }

=== Key Insight ===
Two interfaces that do not fit each other, and one object that fits them together.
Neither side is edited and neither knows the other exists, so the client never
carries the adaptee's key shape.
```

## The Code

### Employee.cs

```csharp
namespace AdapterDesignPattern;

public record Employee(int Id, string Name);
```

### IEmployeeReader.cs — Target

```csharp
namespace AdapterDesignPattern;

/// <summary>
/// TARGET — what the client asks for. An id and nothing else: the tenant is already implied by the
/// connection, the way every service was written before its tables moved to PostgreSQL.
/// </summary>
public interface IEmployeeReader
{
    Task<Employee?> ReadAsync(int id);
}
```

### IEmployeeDirectory.cs — Adaptee

```csharp
namespace AdapterDesignPattern;

/// <summary>
/// ADAPTEE — what <c>rigo_hr</c> speaks. A tenant row is keyed <c>(tenant_id, id)</c>, so a find
/// needs the tenant handed to it.
/// </summary>
public interface IEmployeeDirectory
{
    Task<Employee?> FindAsync(Guid tenantId, int id);
}
```

### EmployeeReaderAdapter.cs — Adapter

```csharp
namespace AdapterDesignPattern;

/// <summary>
/// ADAPTER — implements the target (<see cref="IEmployeeReader"/>) by wrapping the adaptee
/// (<see cref="IEmployeeDirectory"/>), and translates the one thing that does not line up.
/// </summary>
/// <remarks>
/// The client's <c>ReadAsync(42)</c> becomes <c>FindAsync(tenantId, 42)</c>. The tenant comes from
/// here, not from the caller: in production the adapter reads the RLS-bound transaction's
/// (<c>PortedRepository&lt;T&gt;</c>, HR.EntityFramework/Shared/Pg/PortedRepository.cs:42, and
/// <c>HrPgRepository&lt;T&gt;.StampTenant</c> do the same), so a service cannot read another
/// tenant's row even if it tries.
/// </remarks>
public sealed class EmployeeReaderAdapter(Guid tenantId, IEmployeeDirectory directory) : IEmployeeReader
{
    public async Task<Employee?> ReadAsync(int id)
    {
        Console.WriteLine($"  [Adapter] ReadAsync({id})  ->  FindAsync({tenantId:D}, {id})");
        return await directory.FindAsync(tenantId, id);
    }
}
```

### FakeDirectory.cs — the adaptee, in memory

```csharp
namespace AdapterDesignPattern;

/// <summary>The adaptee, in memory. It answers SQL and returns whoever the caller is a tenant for.</summary>
public sealed class FakeDirectory : IEmployeeDirectory
{
    private readonly Dictionary<Guid, Dictionary<int, Employee>> _rows = new();

    public void Seed(Guid tenantId, params Employee[] employees)
        => _rows[tenantId] = employees.ToDictionary(e => e.Id);

    public Task<Employee?> FindAsync(Guid tenantId, int id)
    {
        Console.WriteLine($"    [rigo_hr] SELECT * FROM hr.employee WHERE tenant_id = '{tenantId:D}' AND id = {id}");
        _rows.TryGetValue(tenantId, out var tenant);
        return Task.FromResult(tenant is not null && tenant.TryGetValue(id, out var employee) ? employee : null);
    }
}
```

### Program.cs

```csharp
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
Console.WriteLine("ADAPTEE   IEmployeeDirectory    FindAsync(tenantId, id)   what rigo_hr speaks\n");

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
```

## Real-World Analogy

Think of a travel power adapter. Your laptop plug (the target) has two prongs; the wall socket (the adaptee) has three holes in a different layout. The adapter sits between them, presenting your plug's shape on one side and the socket's shape on the other. Your laptop never learns what the socket looks like, and the socket never learns what your plug looks like.

## When to Use

- A class needs to use an existing collaborator whose interface does not match what it calls
- You want to reuse a class without editing it — it is foreign, generated, or frozen
- You are introducing a new dependency behind the interfaces your code already speaks, so the swap is invisible to callers
- You are bridging two systems that were built against different contracts (the real-world case: SQL Server per-tenant services in front of a shared, tenant-keyed PostgreSQL database)
