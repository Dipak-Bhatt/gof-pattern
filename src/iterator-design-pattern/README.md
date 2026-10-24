# Iterator Design Pattern

## Definition

The Iterator pattern **provides a way to access the elements of an aggregate object sequentially
without exposing its underlying representation**. The collection owns the data; the iterator owns
the position. The client asks for the next element and never learns how the collection stores or
orders it.

This example walks the tenants of a scheduled run:

- The run order is sorted by tenant id and rotated per time slot.
- A run has a time budget, so it may start only some of the tenants.
- The same traversal also has to stop when the caller cancels.

The iterator carries all of that: the position, the rotation, the budget and the cancellation
check. The aggregate's array never leaves the aggregate.

## The Example

A scheduler wakes up in a slot and visits tenants. Every tenant must be first in turn, so the
order rotates with the slot:

```text
slot 0:  Acme Foods -> Northwind Bank -> Contoso Textiles -> Fabrikam Logistics -> Tailspin Hotels
slot 1:  Northwind Bank -> Contoso Textiles -> Fabrikam Logistics -> Tailspin Hotels -> Acme Foods
slot 2:  Contoso Textiles -> Fabrikam Logistics -> Tailspin Hotels -> Acme Foods -> Northwind Bank
```

The run has 15 minutes. Suppose each tenant takes 7 minutes:

| Tenant started | Time when it starts | Elapsed after its work |
|----------------|---------------------|------------------------|
| Contoso Textiles | 0 min | 7 min |
| Fabrikam Logistics | 7 min | 14 min |
| Tailspin Hotels | 14 min | 21 min |
| Acme Foods | skipped | — |
| Northwind Bank | skipped | — |

The first tenant is exempt from the budget, so a run always makes progress. After that, the
iterator stops as soon as the elapsed time reaches the budget.

## Structure

```text
┌──────────────────────┐   asks for a cursor   ┌──────────────────────────────┐
│ Client               │──────────────────────>│ ITenantAggregate             │
│ Program.cs           │                       │ CreateIterator(...)          │
└──────────┬───────────┘                       └──────────────┬───────────────┘
           │                                                  │ implemented by
           │                                                  ▼
           │                                  ┌──────────────────────────────┐
           │                                  │ TenantRun                    │
           │                                  │ private Tenant[] _sorted     │
           │                                  │ Order(slot): sort + rotate   │
           │                                  └──────────────┬───────────────┘
           │ pulls one tenant at a time                      │ creates
           ▼                                                 ▼
┌──────────────────────┐                       ┌──────────────────────────────┐
│ ITenantIterator      │<──────────────────────│ TenantRunIterator            │
│ HasNext / Next /Reset│       returns         │ position + budget + cancel   │
└──────────────────────┘                       └──────────────────────────────┘
```

The same aggregate can also be walked with `foreach`. `TenantRun.Startable` uses `yield return`,
which the C# compiler turns into an iterator state machine:

```text
foreach (var tenant in run.Startable(slot, ...))
{
    // one tenant at a time
}
```

## Pattern Roles

| Role | Type | Responsibility |
|------|------|----------------|
| **Aggregate** | `ITenantAggregate` | Declares the method that creates a cursor |
| **Concrete Aggregate** | `TenantRun` | Owns the tenants, sorts and rotates them, creates the iterator |
| **Iterator** | `IIterator<T>`, `ITenantIterator` | Declares `HasNext`, `Next` and `Reset` |
| **Concrete Iterator** | `TenantRunIterator` | Tracks the position and applies the budget and cancellation rules |
| **Client** | `Program.cs` | Pulls tenants through the iterator and never sees the array |
| **C# idiom** | `TenantRun.Startable` | The same traversal as `yield return`; the compiler writes the iterator |

## Project Layout

```text
iterator-design-pattern/
├── Aggregate/
│   ├── ITenantAggregate.cs      — AGGREGATE
│   └── TenantRun.cs             — CONCRETE AGGREGATE
├── Iterator/
│   ├── IIterator.cs             — ITERATOR (generic)
│   ├── ITenantIterator.cs       — ITERATOR (tenant-specific)
│   └── TenantRunIterator.cs     — CONCRETE ITERATOR
├── Infrastructure/
│   └── VirtualTimeProvider.cs   — deterministic clock for the demo
├── Models/
│   └── Tenant.cs                — the element type
├── Program.cs                   — CLIENT and demonstration
├── iterator-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/iterator-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```text
=== Iterator Design Pattern — Tenant Run ===

AGGREGATE  TenantRun            owns the ordered tenants; its array stays private
ITERATOR   IIterator<Tenant>    HasNext / Next / Reset
CONCRETE   TenantRunIterator    stops on the time budget or cancellation
IDIOM      TenantRun.Startable  the same walk with yield return

--- A run in slot 2 rotates the sorted tenants ---
  Contoso Textiles -> Fabrikam Logistics -> Tailspin Hotels -> Acme Foods -> Northwind Bank

--- Forward traversal (HasNext / Next), 15-minute budget, 7 minutes per tenant ---
  started Contoso Textiles     at + 0 min
  started Fabrikam Logistics   at + 7 min
  started Tailspin Hotels      at +14 min
  stopped after 3 tenants: +21 min >= 15
  not started: Acme Foods, Northwind Bank

--- The C# idiom: foreach over yield return ---
  started Contoso Textiles     at + 0 min
  started Fabrikam Logistics   at + 7 min
  started Tailspin Hotels      at +14 min

--- Two cursors over one aggregate keep separate positions ---
  cursor A: Contoso Textiles
  cursor B: Contoso Textiles
  cursor A: Fabrikam Logistics   (B is still on its first)

--- Reset returns a cursor to the start ---
  after two steps: Tailspin Hotels
  after Reset:     Contoso Textiles

--- Cancellation stops the walk before the next tenant ---
  started Contoso Textiles
  started Fabrikam Logistics
  cancelled after 2 of 5 tenants

=== Key Insight ===
The client never sees TenantRun's private array or its rotation index. It asks the
aggregate for a cursor and pulls one tenant at a time. The iterator's stop rules
(time budget, cancellation) travel with the cursor, so the same aggregate can be
walked by several callers at once. In C#, foreach over yield return is this pattern
with the iterator class generated by the compiler; the explicit class here shows the
roles the language normally hides.
```

## How It Works

1. **The aggregate hides its storage.** `TenantRun` keeps the tenants in a private array. It
   exposes `Count` and `Order(slot)`, but not the array itself.
2. **Order is computed once per cursor.** `Order(slot)` sorts the tenants by id, drops duplicates
   and empty ids, then rotates the list so `slot % Count` is first.
3. **The aggregate creates a cursor.** `CreateIterator` returns a `TenantRunIterator` positioned
   before the first tenant. The iterator gets the ordered list and the stop rules.
4. **The client pulls.** `HasNext` checks three things: is there another tenant, has the caller
   cancelled, and has the budget run out for a tenant after the first. `Next` returns the current
   tenant and advances the position.
5. **Each cursor is independent.** Two calls to `CreateIterator` create two objects with two
   positions. Advancing one does not move the other.
6. **Reset rewinds.** `Reset` sets the position back to zero without rebuilding the order.
7. **The language can generate it.** `Startable` expresses the same traversal with `yield return`
   and `yield break`. `foreach` then calls `MoveNext` and `Current` on the generated state machine.

## Why an Iterator Here?

The client needs to visit tenants one at a time, and the decision to continue depends on work that
happens between visits. Loading all tenants into a list and looping over them would still work, but
it would make the stop rules the client's job. The iterator keeps them in one place:

- **Lazy by construction.** The clock is read between tenants, so a tenant that will not be started
  is never handed out.
- **Encapsulated traversal.** The rotation index and the backing array stay private.
- **Multiple traversals.** Several cursors can walk the same aggregate without sharing state.
- **Interchangeable strategies.** A different order is a different iterator class; the client code
  does not change.
- **Stop rules travel with the cursor.** Budget and cancellation are not duplicated at call sites.

## Software Design Principles

| Principle | How this example applies it |
|-----------|-----------------------------|
| **Single Responsibility** | `TenantRun` stores and orders; `TenantRunIterator` tracks position and stop rules; `Program` consumes. |
| **Open/Closed** | A new traversal (reverse, filtered, round-robin) is a new iterator class; the aggregate and client stay put. |
| **Encapsulation** | The private array is never exposed; clients cannot index into it or share a position. |
| **Liskov Substitution** | Any `ITenantIterator` can replace `TenantRunIterator` without changing the loop. |
| **Dependency Inversion** | The client depends on the iterator and aggregate interfaces, not on concrete storage. |
| **Composition over duplication** | Each cursor composes the ordered list with its own position and rules. |

## Real-World Analogy

A tour bus follows a route with several stops. The route (the aggregate) knows the stops and their
order. The guide (the iterator) knows which stop the group is at, which stop is next, and when the
tour must end. Passengers ask the guide "where are we next?"; they never read the route map
themselves. Two guides on two buses can follow the same route independently.

## When to Use

- You need to traverse a collection without exposing its internal structure.
- The same collection needs more than one traversal order or strategy.
- Several clients must traverse the same collection at the same time without interfering.
- The traversal has its own rules: paging, a time budget, filtering or cancellation.
- You want to change the traversal without changing the aggregate or the client.

## When Not to Use

- A simple array or `List<T>` with `foreach` already expresses the traversal clearly.
- The collection is tiny and used in one place; a custom iterator is extra indirection.
- The traversal must be very fast; a hand-written index loop may be simpler and cheaper.
- The rules change per call in ways the iterator cannot own; a plain loop with a predicate is
  easier to read.
