# Bridge Design Pattern

## Definition

The Bridge pattern **decouples an abstraction from its implementation** so that the two can vary independently. Instead of inheriating implementation details, the abstraction holds a reference to an interface and delegates work to it.

## Structure

```
┌─────────────────────┐         ┌─────────────────────────┐
│   Abstraction       │────────>│   Implementor (iface)   │
│   EmployeeService   │  holds  │   IEmployeeRepository   │
└─────────────────────┘         └───────────┬─────────────┘
                                            │
                        ┌───────────────────┴───────────────────┐
                        │                                       │
              ┌─────────────────────┐               ┌─────────────────────┐
              │ ConcreteImplementor │               │ ConcreteImplementor │
              │ SqlServerRepository │               │ PostgresRepository  │
              └─────────────────────┘               └─────────────────────┘
```

## Project Layout

```
bridge-design-pattern/
├── Domain/
│   ├── Employee.cs              — shared entity
│   └── DatabaseEngine.cs        — enum for engine type
├── Abstraction/
│   ├── IEmployeeRepository.cs   — Implementor interface
│   └── EmployeeService.cs       — Abstraction (depends on interface)
├── Implementation/
│   ├── SqlServerEmployeeRepository.cs
│   └── PostgresEmployeeRepository.cs
├── Program.cs                   — composition root / DI wiring
└── README.md
```

## How to Run

```bash
dotnet run
```

## Output

```
=== Bridge Pattern Demo — Multi-Engine Architecture ===

--- Tenant on SqlServer ---
  [Service] Fetching employee 42 via SQL Server (per-tenant)
  [SQL Server] SELECT * FROM Employees WHERE EmployeeId = 42
  Result: Ram Bahadur, Engineering, $75,000

  [Service] Hiring Sita Sharma via SQL Server (per-tenant)
  [SQL Server] INSERT INTO Employees (Name, Dept) VALUES ('Sita Sharma', 'HR')

--- Tenant on PostgreSQL ---
  [Service] Fetching employee 42 via PostgreSQL (shared, RLS)
  [PostgreSQL] SELECT * FROM employees WHERE employee_id = 42 AND tenant_id = current_tenant()
  Result: Ram Bahadur, Engineering, $75,000

  [Service] Hiring Sita Sharma via PostgreSQL (shared, RLS)
  [PostgreSQL] INSERT INTO employees (name, department) VALUES ('Sita Sharma', 'HR')

=== Key Insight ===
EmployeeService never knows which database it talks to.
Swap the IEmployeeRepository implementation -> service code unchanged.
Abstraction (service) is decoupled from implementation (repository + engine).
```

## Real-World Analogy

Think of a TV remote and a TV. The remote (abstraction) defines buttons like "power" and "volume". Each TV brand (implementation) receives those commands but executes them differently. You can swap the TV without changing the remote, and add new TV models without changing the remote's interface.

## When to Use

- You want to avoid a permanent binding between abstraction and implementation
- Both abstractions and implementations should be extensible via subclassing
- Changes in implementation should not affect clients
- You need to share an implementation among multiple objects
