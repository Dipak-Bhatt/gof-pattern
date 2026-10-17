# Singleton Design Pattern

## Definition

The Singleton pattern ensures a class has **exactly one instance** and provides a single global point of access to it. It is used when a single, shared, process-wide resource must be coordinated — a connection pool, a cache, a configuration manager — and constructing it more than once would be wrong or wasteful.

Three forms are shown, from simplest to most used:

1. **Classic, for beginners** — the class itself holds the one instance (`static Instance`). Start here to learn the idea.
2. **Thread-safe classic** — the same, guarded by `Lazy<T>` so it is created on first use and is thread-safe (`TenantConnectionPool`).
3. **DI singleton** — the container owns the single instance and hands the same object to every scope (the form used throughout the .NET ecosystem).

## Classic Singleton for Beginners

Start with the smallest possible version. A logger is a familiar example: every part of an app wants to write to it, but there should be only one.

```csharp
public sealed class SimpleLogger
{
    // 1. one private static field holds the single instance
    private static SimpleLogger? _instance;

    // 2. a private constructor, so nobody outside can write `new SimpleLogger()`
    private SimpleLogger() { }

    // 3. the one public way to get it — create it if needed, then return it
    public static SimpleLogger Instance
    {
        get
        {
            _instance ??= new SimpleLogger();
            return _instance;
        }
    }
}
```

That is the whole pattern. Three ideas:

| Idea | Code | Why |
|------|------|-----|
| Store the one instance | `private static SimpleLogger? _instance;` | `static` means "one per process", not "one per object". |
| Block `new` | `private SimpleLogger() { }` | If the constructor is private, only the class itself can create it. |
| Hand it out | `public static SimpleLogger Instance { get { ... } }` | The single, well-known door to the object. |

`??=` means "if it is null, create it; otherwise leave it". The first caller creates the logger; every later caller gets the same one.

> **A word on threads.** The code above is fine for learning and for single-threaded use. If two threads can call `Instance` at the exact same moment, both might create an instance. The real example (`TenantConnectionPool`, below) fixes that with `Lazy<T>`, which the runtime makes thread-safe for you.

## Structure

```
┌──────────────────────────────┐
│      TenantConnectionPool    │
│  (sealed)                    │
│  - static Lazy<...> _instance│
│  - private ctor              │
│  + static Instance           │
│  + Open()                    │
└──────────────┬───────────────┘
               │ one instance, shared by all callers
       ┌───────┴────────┐
       ▼                ▼
   Program.cs      DI container
 (classic use)   (scoped consumers)
```

## Project Layout

```
singleton-design-pattern/
├── Domain/
│   ├── SimpleLogger.cs                         — the beginner's classic Singleton
│   └── TenantConnectionPool.cs                 — thread-safe classic Singleton (Lazy<T>)
├── DependencyInjection/
│   └── SingletonServiceCollectionExtensions.cs — registers it with the DI container
├── Program.cs                                  — composition root / demo
└── README.md
```

## How to Run

```bash
dotnet run
```

## Output

```
=== Singleton Pattern Demo ===

--- 1. Classic Singleton, explained simply ---
Two different places ask for the logger.

  [LOG] App started
  [LOG] User logged in

Same object? True
Messages stored in the one logger: 2

--- 2. Thread-safe classic Singleton (Lazy<T>) ---
Same instance: True
Connections opened through it: 1 then 2

--- 3. DI singleton (the form .NET apps use) ---
Same across scopes: True
Connections opened through DI: 3 then 4
```

## Code

### Domain/SimpleLogger.cs — the beginner's classic Singleton

```csharp
namespace SingletonDesignPattern.Domain;

/// <summary>
/// The simplest possible Singleton — a good place to learn the pattern.
///
/// A logger that every part of the app can use, but there is only ONE.
/// The whole pattern is three small ideas:
///   1. a private static field that holds the one instance,
///   2. a private constructor so nobody outside can write <c>new SimpleLogger()</c>,
///   3. one public <c>Instance</c> property that creates it once and returns it.
/// </summary>
public sealed class SimpleLogger
{
    // Step 1: one private static field holds the single instance.
    // It starts out null — the instance is created only when first needed.
    private static SimpleLogger? _instance;

    // Step 2: a private constructor, so nobody outside can write `new SimpleLogger()`.
    private SimpleLogger()
    {
        Messages = new List<string>();
    }

    // Step 3: the one public way to get it — create it if needed, then return it.
    public static SimpleLogger Instance
    {
        get
        {
            _instance ??= new SimpleLogger();
            return _instance;
        }
    }

    /// <summary>Every message written through the one logger.</summary>
    public List<string> Messages { get; }

    public void Log(string message)
    {
        Messages.Add(message);
        Console.WriteLine($"  [LOG] {message}");
    }
}
```

### Domain/TenantConnectionPool.cs — the classic Singleton

```csharp
namespace SingletonDesignPattern.Domain;

/// <summary>
/// The classic Singleton: one instance per process. The pool is an expensive,
/// process-wide resource, so it must never be constructed twice.
/// </summary>
public sealed class TenantConnectionPool
{
    // Lazy + ExecutionAndPublication makes the one-instance guarantee thread-safe
    // without a lock on every access.
    private static readonly Lazy<TenantConnectionPool> _instance =
        new(() => new TenantConnectionPool(), LazyThreadSafetyMode.ExecutionAndPublication);

    private int _openConnections;

    // Private constructor: nothing outside the class can call `new`.
    private TenantConnectionPool()
    {
        _openConnections = 0;
    }

    /// <summary>The one instance.</summary>
    public static TenantConnectionPool Instance => _instance.Value;

    /// <summary>Opens a connection on the shared pool. Thread-safe.</summary>
    public int Open()
    {
        return Interlocked.Increment(ref _openConnections);
    }
}
```

The four rules of the classic pattern are all visible here:

- `sealed` — cannot be subclassed (a subclass could break the guarantee).
- `private` constructor — nobody else can call `new`.
- one `static` field (`_instance`) — the single storage.
- one `static` accessor (`Instance`) — the single entry point.

### DependencyInjection/SingletonServiceCollectionExtensions.cs — the DI form

```csharp
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
```

### Program.cs — the demo

```csharp
using Microsoft.Extensions.DependencyInjection;
using SingletonDesignPattern.DependencyInjection;
using SingletonDesignPattern.Domain;

Console.WriteLine("=== Singleton Pattern Demo ===\n");

// 1. Classic Singleton, explained simply.
//    Two different places ask for the logger. They get the SAME object.
Console.WriteLine("--- 1. Classic Singleton, explained simply ---");
Console.WriteLine("Two different places ask for the logger.\n");

var loggerA = SimpleLogger.Instance;
var loggerB = SimpleLogger.Instance;

loggerA.Log("App started");
loggerB.Log("User logged in");

Console.WriteLine();
Console.WriteLine($"Same object? {ReferenceEquals(loggerA, loggerB)}");
Console.WriteLine($"Messages stored in the one logger: {loggerA.Messages.Count}\n");

// 2. Thread-safe classic Singleton (Lazy<T>).
//    The connection pool is a real resource, so it is built once and safely.
Console.WriteLine("--- 2. Thread-safe classic Singleton (Lazy<T>) ---");
var first = TenantConnectionPool.Instance;
var second = TenantConnectionPool.Instance;
Console.WriteLine($"Same instance: {ReferenceEquals(first, second)}");
Console.WriteLine($"Connections opened through it: {first.Open()} then {second.Open()}\n");

// 3. The DI singleton — the form .NET apps actually use.
//    Register once; every scope gets the same object.
Console.WriteLine("--- 3. DI singleton (the form .NET apps use) ---");
var services = new ServiceCollection();
services.AddTenantConnectionPool();
var provider = services.BuildServiceProvider();

using (var scopeA = provider.CreateScope())
using (var scopeB = provider.CreateScope())
{
    var fromA = scopeA.ServiceProvider.GetRequiredService<TenantConnectionPool>();
    var fromB = scopeB.ServiceProvider.GetRequiredService<TenantConnectionPool>();
    Console.WriteLine($"Same across scopes: {ReferenceEquals(fromA, fromB)}");
    Console.WriteLine($"Connections opened through DI: {fromA.Open()} then {fromB.Open()}");
}
```

## Real-World Analogy

Think of a single shared printer in an office. Everyone prints to "the printer", not to a personal copy. The office keeps one printer (one instance) and hands out a queue (access) rather than buying a printer for every person.

## When to Use

- Exactly one instance of a class is required, and it must be accessible from a well-known point.
- The instance is expensive to create or holds a process-wide resource (a pool, a cache, a clock).
- You want a single point of control over shared state.

## When *Not* to Use

- The class holds per-request or per-tenant state — a singleton would leak it across callers.
- You need to swap implementations at runtime — prefer dependency injection instead.
- Testability matters: a static `Instance` is hard to fake. The DI form is preferred for that reason.
