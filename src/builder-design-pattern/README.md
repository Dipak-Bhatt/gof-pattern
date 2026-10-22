# Builder Design Pattern

## Definition

The Builder pattern **separates the construction of a complex object from its representation**.
The client names the parts it knows, one fluent call at a time; the builder holds the partially
assembled object, validates it, and returns a finished product. The product is immutable, so no
consumer can change it after construction.

Use it when a type has many optional parts, when the order of assignment does not matter, and when
a half-built object must never escape to a consumer.

This example builds an **audit event**:

```csharp
director.Update()
    .SetCategory(AuditCategory.Employee)
    .SetTarget(42, "Ram Bahadur")
    .SetBefore(oldState)
    .SetAfter(newState)
    .Publish();
```

The call site supplies only the parts it has. It never constructs `AuditEvent` directly, never
repeats validation, and cannot leave a required field unset.

## Structure

```
┌──────────────────────┐
│  Client              │
│  Program.cs          │
└──────────┬───────────┘
           │ director.Update().SetCategory(...).SetTarget(...).Publish()
           ▼
┌──────────────────────────────────────────┐
│  DIRECTOR                                │
│  AuditEventDirector                      │
│  - Add() / Update() / Delete() / ...     │
│  - starts a builder with the action set  │
└──────────┬───────────────────────────────┘
           │ new AuditEventBuilder(logger, action)
           ▼
┌──────────────────────────────────────────┐
│  BUILDER                                 │
│  AuditEventBuilder                       │
│  - holds the partially built event       │
│  - one Set... method per optional part   │
│  - Build() validates and freezes         │
│  - Publish() builds and sends            │
└──────────┬───────────────────────────────┘
           │ Build()
           ▼
┌──────────────────────────┐        ┌──────────────────┐
│  PRODUCT                 │        │  RECEIVER        │
│  AuditEvent (immutable)  │───────▶│  IAuditLogger    │
└──────────────────────────┘        └──────────────────┘
```

## Pattern Roles

| Role | Type | Responsibility |
|------|------|----------------|
| **Product** | `AuditEvent`, `AuditTarget` | The immutable object being built |
| **Builder** | `AuditEventBuilder` | Accumulates parts, validates, returns the product |
| **Concrete Builder** | `AuditEventBuilder` | The only representation in this demo |
| **Director** | `AuditEventDirector` | Starts a build with the action pre-selected |
| **Receiver** | `IAuditLogger`, `ConsoleAuditLogger` | Consumes the finished product |
| **Client** | `Program.cs` | Chains only the parts it knows |

## Project Layout

```
builder-design-pattern/
├── AuditCategory.cs        — PRODUCT enum
├── AuditEvent.cs           — PRODUCT (immutable event)
├── AuditTarget.cs          — PRODUCT part (what the action was performed on)
├── AuditEventBuilder.cs    — BUILDER / CONCRETE BUILDER
├── AuditEventDirector.cs   — DIRECTOR (action-specific entry points)
├── IAuditLogger.cs         — RECEIVER interface
├── ConsoleAuditLogger.cs   — demo receiver
├── LogAction.cs            — PRODUCT enum
├── Program.cs              — client / demo scenarios
├── builder-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/builder-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```
=== Builder Pattern — audit events ===

DIRECTOR  AuditEventDirector.Add() / Update() / ...
BUILDER   AuditEventBuilder.Set...() then Build() or Publish()
PRODUCT   AuditEvent (immutable)

--- Add: target id and metadata ---
[audit] Added      Department      target=12 (Department) tenant=11111111-1111-1111-1111-111111111111
        metadata={"Name":"Engineering","Code":"ENG"}

--- Update: before/after objects become JSON ---
[audit] Updated    Employee        target=42 (Ram Bahadur) employee=7 message="Department changed"
        before={"Name":"Ram Bahadur","DepartmentId":7} after={"Name":"Ram Bahadur","DepartmentId":12}

--- Update with no real change: state is omitted ---
[audit] Updated    Employee        target=42 (Ram Bahadur)

--- Upload: target name defaults from the category ---
[audit] Uploaded   Payslip         target=payslip-2026-10.pdf (Payslip)

--- Delete ---
[audit] Deleted    Role            target=99 (Payroll Admin)

--- Validation: forgetting the category fails at Build ---
  rejected: An audit category must be set before building.

=== Key Insight ===
The builder assembles and validates; the immutable event is the product.
Each call site chains only the parts it has, and Publish() hands it to the receiver.
```

## Rules Enforced in `Build()`

- `Category` and `Action` are required; `Build()` throws if either is missing.
- `Added`, `Downloaded` and `Uploaded` default the target name from the category.
- `Updated` drops before/after state when either side is missing or both sides are equal.

## Real-World Analogy

Ordering a sandwich: you name the bread, filling, cheese, salad and sauce you want, in any order,
and the counter assembles one finished sandwich. You never receive a half-made one, and you never
have to remember which combinations are valid — the person assembling checks before handing it
over. The finished sandwich leaves the counter and is not changed afterwards.

## When to Use

- An object has many optional parts and a telescoping constructor would be unreadable
- The same construction steps produce different representations or configurations
- Parts may be set in a different order at different call sites
- A partially built object must never escape to a consumer
- Validation is the same for every call site and belongs next to construction

## When Not to Use

- The object has two or three required values and one obvious constructor
- The object is naturally mutable and is edited after construction
- Construction needs no validation and no fluent readability
- You would be adding a builder only to wrap a single `new` call
