# Decorator Design Pattern

## Definition

The Decorator pattern **adds behaviour to an object without changing its class**. It wraps the object in another object that implements the *same interface*, forwards each call to the wrapped instance, and does extra work before, after, or around that call. The client holds the interface, so it cannot tell a plain object from a stack of decorators. Behaviours are composed at the call site, not baked into inheritance.

This is the structural alternative to subclassing: instead of one class per combination (`CachedAuditedReader`, `RetryingAuditedSender`, …), you compose small decorators in the order you want.

## Structure

```
┌──────────────────┐
│     Client       │
└────────┬─────────┘
         │ holds
         ▼
┌──────────────────────────────┐
│  Component (interface)       │
│  IEmployeeReader             │◀──────────────┐
└──────────────▲───────────────┘               │
               │ implements                    │ implements
   ┌───────────┴────────────┐      ┌───────────┴──────────────────┐
   │ Concrete Component     │      │ Base Decorator               │
   │ EmployeeReader         │      │ EmployeeReaderDecorator      │
   └────────────────────────┘      │  - Inner : IEmployeeReader   │
                                   └───────────▲──────────────────┘
                                               │ extends
                         ┌─────────────────────┴─────────────────────┐
                         │ CachingEmployeeReaderDecorator             │
                         │ AuditingEmployeeReaderDecorator            │
                         └────────────────────────────────────────────┘
```

At runtime the client sees a decorator as an `IEmployeeReader`. The decorator holds another `IEmployeeReader` — which may itself be a decorator. That nesting is the whole pattern.

## Project Layout

```
decorator-design-pattern/
├── Employee.cs                                  — shared record (scenario 1)
├── IEmployeeReader.cs                            — COMPONENT interface
├── EmployeeReader.cs                             — concrete component
├── EmployeeReaderDecorator.cs                    — base decorator (holds Inner)
├── CachingEmployeeReaderDecorator.cs             — concrete decorator: cache
├── AuditingEmployeeReaderDecorator.cs            — concrete decorator: tenant audit
├── NotificationIntent.cs                         — shared record (scenario 2)
├── INotificationSender.cs                        — COMPONENT interface
├── OutboxNotificationSender.cs                   — concrete component (simulated outbox)
├── NotificationSenderDecorator.cs                — base decorator
├── RetryingNotificationSenderDecorator.cs        — concrete decorator: retry
├── AuditNotificationSenderDecorator.cs           — concrete decorator: audit
├── MetricsNotificationSenderDecorator.cs         — concrete decorator: metrics
├── Program.cs                                    — composition root / wiring
└── README.md
```

## How to Run

```bash
cd src/decorator-design-pattern
dotnet run
```

## Output

```
=== Decorator Pattern — Scenario 1: cross-cutting read concerns ===

The client holds ONE interface, IEmployeeReader. It cannot tell a plain reader from
a stack of decorators — that is the point of the pattern.

--- Plain component ---
    [rigo_hr] SELECT * FROM hr.employee WHERE id = 42
  Employee { Id = 42, Name = Ram Bahadur }
    [rigo_hr] SELECT * FROM hr.employee WHERE id = 42
  Employee { Id = 42, Name = Ram Bahadur }   // read again: rigo_hr is hit twice

--- Wrapped with caching (outer) then auditing (inner) ---
  [Audit] tenant=11111111-1111-1111-1111-111111111111 read id=42
  [Cache] MISS id=42 — asking the wrapped reader
    [rigo_hr] SELECT * FROM hr.employee WHERE id = 42
  Employee { Id = 42, Name = Ram Bahadur }
  [Audit] tenant=11111111-1111-1111-1111-111111111111 read id=42
  [Cache] HIT  id=42 -> Ram Bahadur
  Employee { Id = 42, Name = Ram Bahadur }   // read again: audited, served from cache
  [Audit] tenant=11111111-1111-1111-1111-111111111111 read id=99
  [Cache] MISS id=99 — asking the wrapped reader
    [rigo_hr] SELECT * FROM hr.employee WHERE id = 99
  not found

--- Same stack, another tenant ---
  [Audit] tenant=99999999-9999-9999-9999-999999999999 read id=42
  [Cache] HIT  id=42 -> Ram Bahadur
  Employee { Id = 42, Name = Ram Bahadur }

=== Decorator Pattern — Scenario 2: notify send path ===

Same shape, different domain and different behaviours. The caller still holds ONE
interface, INotificationSender; the decorators stack in any order.

--- Stack A: Metrics( Audit( Retry( outbox ) ) ) — retry closest to the outbox ---
  [Audit] sending 'hr.payroll.payslip' (payslip-2026-10-19)
    [outbox] INSERT INTO workhorse_outbox (type, key) VALUES ('hr.payroll.payslip', 'payslip-2026-10-19')
  [Retry] attempt 1 refused 'hr.payroll.payslip' (payslip-2026-10-19) — retrying
    [outbox] INSERT INTO workhorse_outbox (type, key) VALUES ('hr.payroll.payslip', 'payslip-2026-10-19')
  [Audit] sent 'hr.payroll.payslip' (payslip-2026-10-19)
  result: True   // transient failure recovered in-process
  metrics: sent=1 refused=0

--- Stack B: Retry( Audit( Metrics( outbox ) ) ) — metrics closest to the outbox ---
  [Audit] sending 'hr.payroll.payslip' (payslip-2026-10-19)
    [outbox] INSERT INTO workhorse_outbox (type, key) VALUES ('hr.payroll.payslip', 'payslip-2026-10-19')
  [Audit] refused 'hr.payroll.payslip' (payslip-2026-10-19)
  [Retry] attempt 1 refused 'hr.payroll.payslip' (payslip-2026-10-19) — retrying
  [Audit] sending 'hr.payroll.payslip' (payslip-2026-10-19)
    [outbox] INSERT INTO workhorse_outbox (type, key) VALUES ('hr.payroll.payslip', 'payslip-2026-10-19')
  [Audit] sent 'hr.payroll.payslip' (payslip-2026-10-19)
  result: True
  metrics: sent=1 refused=1   // counts the retry's attempts

=== Key Insight ===
One component, many optional behaviours, composed at the call site. Each decorator
implements the same interface and forwards to the one it wraps, so they stack in any
order and the client never knows. In Scenario 2 the order matters: metrics on the
outside counts the outcome once; metrics on the inside counts every retry attempt.
Real home: an IRepository<T> or IHrNotifications wrapped with caching, audit, retry or
metrics without editing the component itself.
```

## The Code

### Scenario 1 — read path

#### Employee.cs

```csharp
namespace DecoratorDesignPattern;

public record Employee(int Id, string Name);
```

#### IEmployeeReader.cs — Component

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// COMPONENT — the interface every reader, plain or decorated, answers. The client depends on
/// this alone, so a decorator is indistinguishable from the object it wraps.
/// </summary>
public interface IEmployeeReader
{
    Task<Employee?> ReadAsync(int id);
}
```

#### EmployeeReader.cs — Concrete Component

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE COMPONENT — the thing that actually reads. In production this is the repository the
/// service already calls; here it is a fake <c>rigo_hr</c> read, so the demo is self-contained.
/// </summary>
public sealed class EmployeeReader : IEmployeeReader
{
    public Task<Employee?> ReadAsync(int id)
    {
        Console.WriteLine($"    [rigo_hr] SELECT * FROM hr.employee WHERE id = {id}");
        var found = id is 42 or 43;
        return Task.FromResult<Employee?>(found ? new Employee(id, id == 42 ? "Ram Bahadur" : "Sita Sharma") : null);
    }
}
```

#### EmployeeReaderDecorator.cs — Base Decorator

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// BASE DECORATOR — holds the wrapped component and forwards every call to it. Concrete
/// decorators derive from here and add behaviour around the call, never changing the contract.
/// </summary>
public abstract class EmployeeReaderDecorator(IEmployeeReader inner) : IEmployeeReader
{
    public IEmployeeReader Inner { get; } = inner;

    public virtual Task<Employee?> ReadAsync(int id) => Inner.ReadAsync(id);
}
```

#### CachingEmployeeReaderDecorator.cs

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE DECORATOR — caches a read so the same id is not answered twice. It is a decorator,
/// not a new reader: it implements <see cref="IEmployeeReader"/> and delegates to the wrapped one
/// only on a miss.
/// </summary>
public sealed class CachingEmployeeReaderDecorator(IEmployeeReader inner) : EmployeeReaderDecorator(inner)
{
    private readonly Dictionary<int, Employee?> _cache = new();

    public override Task<Employee?> ReadAsync(int id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            Console.WriteLine($"  [Cache] HIT  id={id} -> {cached?.Name ?? "not found"}");
            return Task.FromResult(cached);
        }

        Console.WriteLine($"  [Cache] MISS id={id} — asking the wrapped reader");
        var result = Inner.ReadAsync(id).GetAwaiter().GetResult();
        _cache[id] = result;
        return Task.FromResult(result);
    }
}
```

#### AuditingEmployeeReaderDecorator.cs

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE DECORATOR — writes a tenant audit line around the read. The tenant is injected, not
/// asked of the caller, exactly as an RLS-bound context carries it.
/// </summary>
public sealed class AuditingEmployeeReaderDecorator(Guid tenantId, IEmployeeReader inner)
    : EmployeeReaderDecorator(inner)
{
    public override Task<Employee?> ReadAsync(int id)
    {
        Console.WriteLine($"  [Audit] tenant={tenantId:D} read id={id}");
        return Inner.ReadAsync(id);
    }
}
```

### Scenario 2 — notify send path

#### NotificationIntent.cs

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// The thing a sender is asked to hand on. Mirrors <c>NotificationIntent</c> in
/// HR.Services/Notify/HrNotifications.cs: a type and an idempotency key, never an address.
/// </summary>
public record NotificationIntent(string Type, string IdempotencyKey);
```

#### INotificationSender.cs — Component

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// COMPONENT — the only interface the caller knows. Real shape: <c>IHrNotifications.SendAsync</c>
/// answers false when an intent was refused or not written, never throws for a business failure.
/// </summary>
public interface INotificationSender
{
    Task<bool> SendAsync(NotificationIntent intent);
}
```

#### OutboxNotificationSender.cs — Concrete Component

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE COMPONENT — writes the intent to <c>workhorse_outbox</c> in <c>rigo_hr</c> (ADR-0012).
/// Fails the first time for an intent to make the retry decorator visible.
/// </summary>
public sealed class OutboxNotificationSender : INotificationSender
{
    private readonly HashSet<string> _attempted = new();

    public Task<bool> SendAsync(NotificationIntent intent)
    {
        Console.WriteLine($"    [outbox] INSERT INTO workhorse_outbox (type, key) VALUES ('{intent.Type}', '{intent.IdempotencyKey}')");

        // A transient failure on the very first attempt, the way a dropped connection behaves:
        // the retry decorator's second call is answered.
        var firstAttempt = _attempted.Add(intent.IdempotencyKey);
        return Task.FromResult(!firstAttempt);
    }
}
```

#### NotificationSenderDecorator.cs — Base Decorator

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// BASE DECORATOR — forwards to the wrapped sender. Every concrete decorator derives from here.
/// </summary>
public abstract class NotificationSenderDecorator(INotificationSender inner) : INotificationSender
{
    public INotificationSender Inner { get; } = inner;

    public virtual Task<bool> SendAsync(NotificationIntent intent) => Inner.SendAsync(intent);
}
```

#### RetryingNotificationSenderDecorator.cs

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE DECORATOR — retries a refused intent. The outbox already gives durability and replay;
/// this is the in-process "try again now" layer for a transient failure.
/// </summary>
public sealed class RetryingNotificationSenderDecorator(INotificationSender inner, int attempts = 3)
    : NotificationSenderDecorator(inner)
{
    public override async Task<bool> SendAsync(NotificationIntent intent)
    {
        for (var attempt = 1; ; attempt++)
        {
            if (await Inner.SendAsync(intent).ConfigureAwait(false))
                return true;

            if (attempt >= attempts)
                return false;

            Console.WriteLine($"  [Retry] attempt {attempt} refused '{intent.Type}' ({intent.IdempotencyKey}) — retrying");
        }
    }
}
```

#### AuditNotificationSenderDecorator.cs

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE DECORATOR — writes an audit line. It logs the type and idempotency key only, never an
/// address, exactly as <c>HrNotifyLog</c> does (event ids 7201–7204, docs/standards/logging.md).
/// </summary>
public sealed class AuditNotificationSenderDecorator(INotificationSender inner) : NotificationSenderDecorator(inner)
{
    public override async Task<bool> SendAsync(NotificationIntent intent)
    {
        Console.WriteLine($"  [Audit] sending '{intent.Type}' ({intent.IdempotencyKey})");
        var sent = await Inner.SendAsync(intent).ConfigureAwait(false);
        Console.WriteLine($"  [Audit] {(sent ? "sent" : "refused")} '{intent.Type}' ({intent.IdempotencyKey})");
        return sent;
    }
}
```

#### MetricsNotificationSenderDecorator.cs

```csharp
namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE DECORATOR — counts what was sent and refused, the cross-cutting metric a caller wants
/// without the component knowing about metrics.
/// </summary>
public sealed class MetricsNotificationSenderDecorator(INotificationSender inner) : NotificationSenderDecorator(inner)
{
    public int Sent { get; private set; }
    public int Refused { get; private set; }

    public override async Task<bool> SendAsync(NotificationIntent intent)
    {
        var sent = await Inner.SendAsync(intent).ConfigureAwait(false);
        if (sent) Sent++; else Refused++;
        return sent;
    }
}
```

### Program.cs — Composition Root

```csharp
using DecoratorDesignPattern;

var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

Console.WriteLine("=== Decorator Pattern — Scenario 1: cross-cutting read concerns ===\n");
Console.WriteLine("The client holds ONE interface, IEmployeeReader. It cannot tell a plain reader from");
Console.WriteLine("a stack of decorators — that is the point of the pattern.\n");

IEmployeeReader plain = new EmployeeReader();

Console.WriteLine("--- Plain component ---");
Console.WriteLine($"  {await plain.ReadAsync(42)}");
Console.WriteLine($"  {await plain.ReadAsync(42)}   // read again: rigo_hr is hit twice\n");

Console.WriteLine("--- Wrapped with caching (outer) then auditing (inner) ---");
IEmployeeReader cached = new CachingEmployeeReaderDecorator(plain);
IEmployeeReader decorated = new AuditingEmployeeReaderDecorator(tenantId, cached);

Console.WriteLine($"  {await decorated.ReadAsync(42)}");
Console.WriteLine($"  {await decorated.ReadAsync(42)}   // read again: audited, served from cache");
Console.WriteLine($"  {await decorated.ReadAsync(99) as object ?? "not found"}");

Console.WriteLine("\n--- Same stack, another tenant ---");
IEmployeeReader otherTenant = new AuditingEmployeeReaderDecorator(
    Guid.Parse("99999999-9999-9999-9999-999999999999"), cached);
Console.WriteLine($"  {await otherTenant.ReadAsync(42)}\n");

// ─────────────────────────────────────────────────────────────────────────────
// Scenario 2 — a send path, decorated with retry, audit and metrics.
// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("=== Decorator Pattern — Scenario 2: notify send path ===\n");
Console.WriteLine("Same shape, different domain and different behaviours. The caller still holds ONE");
Console.WriteLine("interface, INotificationSender; the decorators stack in any order.\n");

var payslip = new NotificationIntent("hr.payroll.payslip", "payslip-2026-10-19");

Console.WriteLine("--- Stack A: Metrics( Audit( Retry( outbox ) ) ) — retry closest to the outbox ---");
var outboxA = new OutboxNotificationSender();
var stackA = new MetricsNotificationSenderDecorator(
    new AuditNotificationSenderDecorator(
        new RetryingNotificationSenderDecorator(outboxA)));
Console.WriteLine($"  result: {await stackA.SendAsync(payslip)}   // transient failure recovered in-process");
Console.WriteLine($"  metrics: sent={stackA.Sent} refused={stackA.Refused}\n");

Console.WriteLine("--- Stack B: Retry( Audit( Metrics( outbox ) ) ) — metrics closest to the outbox ---");
var outboxB = new OutboxNotificationSender();
var stackB = new RetryingNotificationSenderDecorator(
    new AuditNotificationSenderDecorator(
        new MetricsNotificationSenderDecorator(outboxB)));
Console.WriteLine($"  result: {await stackB.SendAsync(payslip)}");
var metricsB = Unwrap(stackB);
Console.WriteLine($"  metrics: sent={metricsB.Sent} refused={metricsB.Refused}   // counts the retry's attempts\n");

Console.WriteLine("=== Key Insight ===");
Console.WriteLine("One component, many optional behaviours, composed at the call site. Each decorator");
Console.WriteLine("implements the same interface and forwards to the one it wraps, so they stack in any");
Console.WriteLine("order and the client never knows. In Scenario 2 the order matters: metrics on the");
Console.WriteLine("outside counts the outcome once; metrics on the inside counts every retry attempt.");
Console.WriteLine("Real home: an IRepository<T> or IHrNotifications wrapped with caching, audit, retry or");
Console.WriteLine("metrics without editing the component itself.");

// Walks in to the metrics decorator wherever it sits in the stack.
static MetricsNotificationSenderDecorator Unwrap(INotificationSender sender) => sender switch
{
    MetricsNotificationSenderDecorator m => m,
    NotificationSenderDecorator d => Unwrap(d.Inner),
    _ => throw new InvalidOperationException("no metrics decorator in this stack")
};
```

## Why Two Scenarios?

The first shows the **transparent stacking** the pattern is known for: caching and auditing are added to a read with no change to `EmployeeReader` or its caller. The second shows the part that surprises people: **order matters**. `Metrics(Audit(Retry(outbox)))` counts the recovered send once; `Retry(Audit(Metrics(outbox)))` counts every attempt. Both are valid; the composition root decides.

## Real-World Analogy

Think of a coffee. The espresso (component) is fixed. Milk, sugar and a shot of vanilla (decorators) each wrap the previous cup and add to it. You can stack them in any order and in any combination, and a waiter carrying the cup (the client) only knows it is holding "a coffee". You do not define a `MilkSugarVanillaCoffee` class for every combination.

## When to Use

- Add responsibilities to individual objects without affecting others of the same class
- Add behaviour that can be withdrawn at runtime — wrap or do not wrap
- Combine several orthogonal concerns (caching, logging, retry, metrics, authorization) without a class explosion
- You cannot or should not edit the component — it is generated, foreign, or frozen
- You want to keep the component focused on its one job and compose the rest around it

## Decorator vs. Related Patterns

- **Adapter** changes the interface so two collaborators fit; a Decorator keeps the same interface.
- **Proxy** also implements the same interface and wraps an object, but usually controls *access* (lazy load, guard, remote call) rather than adding behaviour; the intent differs.
- **Composite** treats a leaf and a tree uniformly; a Decorator wraps exactly one component.
- **Strategy** swaps the whole algorithm; a Decorator layers around an existing one.

## Notes on This Repo

The examples are grounded in a real HR domain (`apps/hr/api`): `IEmployeeReader` mirrors a repository read, and `INotificationSender` mirrors `IHrNotifications` in `HR.Services/Notify/HrNotifications.cs`. In the real codebase the decorator pattern is **not yet used**; these demos show where it fits — an `IRepository<T>` or `IHrNotifications` wrapped with caching, tenant audit, retry or metrics.
