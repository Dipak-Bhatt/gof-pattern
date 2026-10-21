# Proxy Design Pattern

## Definition

The Proxy pattern gives a **stand-in object the same interface as the real one** and puts it in
front of the real object. The client holds the interface and cannot tell the proxy from the
subject. What the proxy *does* with that position is what the variants differ in:

- **Routing Proxy** — decides **where** a call goes: forwards it to one of several
  implementations based on a per-call condition.
- **Protection Proxy** — decides **whether** a call is allowed: checks a permission and refuses
  the caller before the real object is ever reached.

Both are the GoF Proxy: same interface, a wrapper in front, the client unchanged. The two
scenarios below show each variant in isolation.

## Structure

```
        Client
           │ holds the interface
           ▼
   ┌───────────────────────────────┐
   │  Subject (interface)          │
   │  IWidgetRepository /          │
   │  IEmployeeRecord              │
   └───────────────▲───────────────┘
                   │ implements
      ┌────────────┴────────────┐
      │                         │
 Real subject              PROXY
 SqlServerWidget…          RoutedRepositoryProxy<T>
 PgWidget…                 EmployeeRecordProxy
                           (chooses / guards)
```

- **Routing Proxy**: the proxy holds the decision (the tenant's engine) and forwards each call to
  the matching real subject.
- **Protection Proxy**: the proxy holds the caller's identity and forwards only the calls the
  caller may make.

## Project Layout

```
proxy-design-pattern/
├── Routing/
│   ├── TenantEngine.cs                  — the per-call routing condition
│   ├── IWidgetRepository.cs             — SUBJECT interface
│   ├── SqlServerWidgetRepository.cs     — real subject (SQL Server side)
│   ├── PgWidgetRepository.cs            — real subject (Postgres side)
│   └── RoutedRepositoryProxy.cs         — ROUTING PROXY (DispatchProxy)
├── Protection/
│   ├── Employee.cs                      — the sensitive record
│   ├── Role.cs                          — who is calling
│   ├── IEmployeeRecord.cs               — SUBJECT interface
│   ├── EmployeeRecord.cs                — real subject
│   └── EmployeeRecordProxy.cs           — PROTECTION PROXY
├── Program.cs                           — composition root / demo
└── README.md
```

## How to Run

```bash
cd src/proxy-design-pattern
dotnet run
```

## Output

```
=== Proxy Pattern — Scenario 1: Routing Proxy ===

The client holds ONE interface, IWidgetRepository. The proxy decides on every
call which implementation answers, from the tenant's engine.

--- Tenant is on SqlServer ---
  engine: sqlserver
  count(0): 1   // routed to SQL Server

--- Same interface, tenant now on Postgres ---
  engine: postgres
  count(0): 1000   // routed to Postgres

=== Proxy Pattern — Scenario 2: Protection Proxy ===

The client holds ONE interface, IEmployeeRecord. The proxy lets the open read
through and refuses salary unless the caller's role may see it.

--- Caller is a Employee ---
  name: Ram Bahadur   // open to everyone
  salary: ACCESS DENIED
  salary update: ACCESS DENIED

--- Caller is a Manager ---
  name: Ram Bahadur   // open to everyone
  salary: 85000.00
  salary update: allowed

--- Caller is a Hr ---
  name: Ram Bahadur   // open to everyone
  salary: 85000.00
  salary update: allowed

=== Key Insight ===
Both scenarios are the same shape: the client holds the interface and never
knows which object is behind it. The ROUTING proxy changes where a call goes;
the PROTECTION proxy decides whether a call is allowed at all.
```

## The Code

### Scenario 1 — Routing Proxy

#### TenantEngine.cs

```csharp
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
```

#### IWidgetRepository.cs — Subject

```csharp
namespace ProxyDesignPattern.Routing;

/// <summary>
/// SUBJECT — the one interface the caller depends on. Both implementations and the proxy answer
/// to it, so the client cannot tell which object is behind the variable.
/// </summary>
public interface IWidgetRepository
{
    /// <summary>Which engine answered. The demo prints this to prove the routing actually happened.</summary>
    string Engine { get; }

    Task<int> CountAsync(int seed);
}
```

#### SqlServerWidgetRepository.cs — Real Subject

```csharp
namespace ProxyDesignPattern.Routing;

/// <summary>
/// REAL SUBJECT (SQL Server side) — the implementation for a tenant still on the old engine.
/// </summary>
public sealed class SqlServerWidgetRepository : IWidgetRepository
{
    public string Engine => "sqlserver";

    public Task<int> CountAsync(int seed) => Task.FromResult(seed + 1);
}
```

#### PgWidgetRepository.cs — Real Subject

```csharp
namespace ProxyDesignPattern.Routing;

/// <summary>
/// REAL SUBJECT (Postgres side) — the implementation for a tenant whose data has moved.
/// </summary>
public sealed class PgWidgetRepository : IWidgetRepository
{
    public string Engine => "postgres";

    public Task<int> CountAsync(int seed) => Task.FromResult(seed + 1000);
}
```

#### RoutedRepositoryProxy.cs — Routing Proxy

```csharp
using System.Reflection;

namespace ProxyDesignPattern.Routing;

/// <summary>
/// ROUTING PROXY — the GoF Proxy pattern used to send a call to one of several implementations.
/// <see cref="DispatchProxy"/> synthesises an <typeparamref name="T"/> at runtime; this class
/// supplies the interception: every method call is redirected to the subject the current
/// tenant's engine names, chosen on each call rather than at construction.
/// </summary>
public class RoutedRepositoryProxy<T> : DispatchProxy where T : class
{
    private Func<TenantEngine> _engine = null!;
    private Func<T> _sqlServer = null!;
    private Func<T> _postgres = null!;

    /// <summary>
    /// Builds the proxy. The engine is a function so the decision is re-read on every call; the
    /// implementations are factories so an unused side is never constructed.
    /// </summary>
    public static T Create(Func<TenantEngine> engine, Func<T> sqlServer, Func<T> postgres)
    {
        var proxy = Create<T, RoutedRepositoryProxy<T>>();
        var self = (RoutedRepositoryProxy<T>)(object)proxy;
        self._engine = engine;
        self._sqlServer = sqlServer;
        self._postgres = postgres;
        return proxy;
    }

    /// <summary>Runs for every call on the interface: forward it to the subject the engine names.</summary>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        => targetMethod!.Invoke(
            _engine() == TenantEngine.Postgres ? _postgres() : _sqlServer(), args);
}
```

### Scenario 2 — Protection Proxy

#### Employee.cs

```csharp
namespace ProxyDesignPattern.Protection;

/// <summary>The sensitive record the proxy guards. Salary is the field that is protected.</summary>
public sealed record Employee(int Id, string Name, decimal Salary);
```

#### Role.cs

```csharp
namespace ProxyDesignPattern.Protection;

/// <summary>Who is calling. The proxy allows the protected members only for some roles.</summary>
public enum Role
{
    Employee,
    Manager,
    Hr
}
```

#### IEmployeeRecord.cs — Subject

```csharp
namespace ProxyDesignPattern.Protection;

/// <summary>
/// SUBJECT — the one interface the caller depends on. The real record and the proxy both answer
/// to it; only the protected members are guarded.
/// </summary>
public interface IEmployeeRecord
{
    /// <summary>Open to every caller — the name is not sensitive.</summary>
    string GetName();

    /// <summary>Protected: only a caller whose role may see salary.</summary>
    decimal GetSalary();

    /// <summary>Protected: only a caller whose role may change salary.</summary>
    void SetSalary(decimal value);
}
```

#### EmployeeRecord.cs — Real Subject

```csharp
namespace ProxyDesignPattern.Protection;

/// <summary>
/// REAL SUBJECT — the plain record. It knows nothing about who is calling; it just answers.
/// </summary>
public sealed class EmployeeRecord : IEmployeeRecord
{
    private Employee _employee;

    public EmployeeRecord(int id, string name, decimal salary)
        => _employee = new Employee(id, name, salary);

    public string GetName() => _employee.Name;

    public decimal GetSalary() => _employee.Salary;

    public void SetSalary(decimal value) => _employee = _employee with { Salary = value };
}
```

#### EmployeeRecordProxy.cs — Protection Proxy

```csharp
namespace ProxyDesignPattern.Protection;

/// <summary>
/// PROTECTION PROXY — the GoF Proxy pattern used to control access. It implements the same
/// interface as the real record, forwards the open call, and checks the caller's role before
/// forwarding the protected ones. A caller without the right role is refused, and the real
/// subject is never reached.
/// </summary>
public sealed class EmployeeRecordProxy(
    IEmployeeRecord inner,
    Guid callerId,
    Role role) : IEmployeeRecord
{
    private readonly IEmployeeRecord _inner = inner;
    private readonly Guid _callerId = callerId;
    private readonly Role _role = role;

    /// <summary>Who may read or change salary. Everyone else is refused.</summary>
    private bool CanSeeSalary => _role is Role.Manager or Role.Hr;

    public string GetName() => _inner.GetName();

    public decimal GetSalary()
    {
        EnsureAllowed();
        return _inner.GetSalary();
    }

    public void SetSalary(decimal value)
    {
        EnsureAllowed();
        _inner.SetSalary(value);
    }

    private void EnsureAllowed()
    {
        if (CanSeeSalary)
            return;

        throw new UnauthorizedAccessException(
            $"Caller {_callerId} with role {_role} may not see or change salary.");
    }
}
```

### Program.cs — Composition Root

```csharp
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
```

## Why Two Scenarios?

The Proxy pattern is one idea with several jobs, and the job is what makes a proxy the right
choice:

- **Routing Proxy** shows the "many implementations behind one interface" job. The condition is
  read on every call, so a tenant that moves engine is routed at once — no re-wiring, no restart.
- **Protection Proxy** shows the "access control" job. The guard sits in front of the subject, so
  the subject itself stays ignorant of permissions and the check cannot be forgotten at a call
  site.

Both keep the client holding only the interface, which is the whole point of the pattern.

## When to Use

- One interface must resolve to different implementations at runtime (routing, sharding,
  A/B, failover, multi-engine support)
- Access to an object must be checked before it is used (authorization, ownership, feature flags)
- The real object must be hidden behind a seam you control (remote call, lazy load, cache,
  audit) — the routing and protection variants are two faces of this
- You cannot or should not edit the real subject — it is foreign, generated, or frozen

## Proxy vs. Related Patterns

- **Adapter** changes the interface so two collaborators fit; a **Proxy** keeps the same interface
  and controls *access or placement*.
- **Decorator** also implements the same interface and wraps an object, but adds behaviour around
  every call; a **Proxy** usually controls *who gets through or where the call goes*. The line is
  intent, not shape.
- **Facade** gives a simple entry point over a subsystem; a **Proxy** stands in for one object.
- **Strategy** swaps the whole algorithm; a **Routing Proxy** picks the object and forwards.

## Notes on This Repo

The two variants are grounded in real seams. A routing proxy is the natural shape for a
repository whose implementation must change with a tenant's assigned engine, and a protection
proxy is the natural shape for a record whose salary (or any sensitive field) must be guarded by
role. In this repository the pattern is shown as two self-contained, runnable demos.
