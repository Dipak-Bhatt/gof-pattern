# Abstract Factory Design Pattern

## Definition

The Abstract Factory pattern **provides one interface for creating a family of related products**
without naming their concrete classes. The client asks a factory for every product it needs, and
the factory decides which engine, platform or variant supplies the whole family.

The pattern has five roles:

| Role | In this demo |
|------|--------------|
| **Abstract Factory** | `IIdentityStoreFactory` — declares both creation methods |
| **Concrete Factory** | `SqlServerIdentityStoreFactory`, `PostgresIdentityStoreFactory` |
| **Abstract Product A** | `IUserDirectory` |
| **Abstract Product B** | `IRoleRepository` |
| **Concrete Products** | `SqlUserDirectory`, `PgUserDirectory`, `SqlRoleRepository`, `PgRoleRepository` |

The client is `UserProvisioningClient`. It holds the abstract factory and the two abstract
products, and it never names a concrete class.

### How this differs from Factory Method

Factory Method returns **one** product and makes the choice once per product. Abstract Factory
returns **two or more related products** and makes the choice once for the whole family.

That matters when the products must agree. If a user directory and a role repository are created by
two independent decisions, a misconfiguration can pair a SQL Server directory with a Postgres role
repository. With an abstract factory, that combination cannot be built: the concrete factory only
knows how to create its own family.

## Structure

```
                       Client
              UserProvisioningClient
                          │
                          │ holds the abstract factory
                          ▼
          ┌────────────────────────────────┐
          │   IIdentityStoreFactory        │  ABSTRACT FACTORY
          │   CreateUserDirectory()        │
          │   CreateRoleRepository()       │
          └───────────────▲────────────────┘
                          │ implemented by
             ┌────────────┴─────────────┐
             │                          │
┌────────────┴─────────────┐  ┌─────────┴────────────────┐
│SqlServerIdentityStore    │  │PostgresIdentityStore     │
│Factory                   │  │Factory                   │
└────────────┬─────────────┘  └─────────┬────────────────┘
             │                          │
             ▼                          ▼
   SqlUserDirectory             PgUserDirectory
   SqlRoleRepository            PgRoleRepository
             │                          │
             └────────────┬─────────────┘
                          ▼
              IUserDirectory · IRoleRepository
                   ABSTRACT PRODUCTS
```

The selector sits above the factories and applies the tenant's current engine:

```
TenantIdentityStoreProvider.For(tenantId)
    ├── SqlServer -> new SqlServerIdentityStoreFactory()
    ├── Postgres  -> new PostgresIdentityStoreFactory()
    └── Frozen    -> refuse, because the migrator holds the tenant
```

The client asks the selector for one factory, then asks that factory for both products. The engine
is therefore read once for the family, not once per interface.

## Project Layout

```
abstract-factory-design-pattern/
├── TenantEngine.cs                         — the variant the family is built for
├── ITenantEngineResolver.cs                — reads that variant / in-memory fake
├── AppUser.cs                              — data returned by a user directory
├── ApplicationRole.cs                      — data returned by a role repository
├── Products/
│   ├── IUserDirectory.cs                   — ABSTRACT PRODUCT A
│   ├── IRoleRepository.cs                  — ABSTRACT PRODUCT B
│   ├── SqlUserDirectory.cs                 — CONCRETE PRODUCT A1
│   ├── SqlRoleRepository.cs                — CONCRETE PRODUCT B1
│   ├── PgUserDirectory.cs                  — CONCRETE PRODUCT A2
│   └── PgRoleRepository.cs                 — CONCRETE PRODUCT B2
├── Factories/
│   ├── IIdentityStoreFactory.cs            — ABSTRACT FACTORY
│   ├── SqlServerIdentityStoreFactory.cs    — CONCRETE FACTORY 1
│   ├── PostgresIdentityStoreFactory.cs     — CONCRETE FACTORY 2
│   └── TenantIdentityStoreProvider.cs      — SELECTOR (engine -> factory)
├── UserProvisioningClient.cs               — CLIENT
├── Program.cs                              — composition root / demo
├── abstract-factory-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/abstract-factory-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```
=== Abstract Factory Pattern — one tenant, one engine, a family of products ===

ABSTRACT FACTORY   IIdentityStoreFactory
ABSTRACT PRODUCTS  IUserDirectory, IRoleRepository
CONCRETE FACTORY   SqlServerIdentityStoreFactory -> SqlUserDirectory + SqlRoleRepository
                   PostgresIdentityStoreFactory  -> PgUserDirectory  + PgRoleRepository
CLIENT             UserProvisioningClient (depends on the abstract factory only)
SELECTOR           TenantIdentityStoreProvider (engine -> concrete factory)

--- Same client, two engines: the whole family changes together ---
    [Selector] tenant 11111111-1111-1111-1111-111111111111 -> SqlServer -> IIdentityStoreFactory
    [SQL Server] SELECT * FROM dbo.AspNetRoles WHERE NormalizedName = 'HR ADMIN'
    [SQL Server] SELECT * FROM dbo.AspNetUsers WHERE NormalizedUserName = 'RAM'
    engine family = sqlserver; Ram Bahadur (ram) gets role HR Admin

    [Selector] tenant 22222222-2222-2222-2222-222222222222 -> Postgres  -> IIdentityStoreFactory
    [Postgres] SELECT * FROM public.identity_role WHERE lower(name) = 'hr admin'
    [Postgres] SELECT * FROM public.identity_users WHERE normalizedusername = 'SITA'
    [Postgres] INSERT INTO public.identity_users (username, fullname) VALUES ('sita', 'Sita Sharma')
    engine family = postgres; Sita Sharma (sita) gets role HR Admin

--- A role outside the family is refused before a user is written ---
    [Selector] tenant 11111111-1111-1111-1111-111111111111 -> SqlServer -> IIdentityStoreFactory
    [SQL Server] SELECT * FROM dbo.AspNetRoles WHERE NormalizedName = 'CHIEF ASTRONAUT'
    role 'Chief Astronaut' does not exist; nothing provisioned

--- Move tenant A to Postgres: both products move in one step ---
    [Selector] tenant 11111111-1111-1111-1111-111111111111 -> Postgres  -> IIdentityStoreFactory
    [Postgres] SELECT * FROM public.identity_role WHERE lower(name) = 'hr admin'
    [Postgres] SELECT * FROM public.identity_users WHERE normalizedusername = 'GITA'
    [Postgres] INSERT INTO public.identity_users (username, fullname) VALUES ('gita', 'Gita Karki')
    engine family = postgres; Gita Karki (gita) gets role HR Admin

--- A frozen tenant gets no family at all ---
  Tenant 22222222-2222-2222-2222-222222222222 is frozen: the migrator holds it, so no identity store may be built.

=== Key Insight ===
Factory Method creates ONE product and leaves the choice per product.
Abstract Factory creates a FAMILY of related products and makes the choice once,
so the user directory and the role repository can never end up on different engines.

The client never names a concrete product or factory. Adding a third engine means
adding one concrete factory and its two products; UserProvisioningClient stays unchanged.
```


## Why Abstract Factory?

- **The family stays consistent.** `UserProvisioningClient` gets both products from the same
  factory, so the user directory and the role repository always read the same engine.
- **The client names no concrete class.** `new SqlUserDirectory()` appears only inside the SQL
  Server factory; `new PgRoleRepository()` appears only inside the Postgres factory.
- **One decision, not many.** The selector reads the tenant's engine once and returns one factory.
  Adding a new engine means adding a factory and its products, not editing every client.
- **Construction can refuse.** A frozen tenant is refused before either product is built, which is
  a rule of creation rather than of reading.
- **A wrong pairing is unrepresentable.** A SQL Server factory has no way to return a Postgres
  product, so the mixed family the client checks for cannot arise by accident.

## When to Use

- The system must work with **multiple families** of related products (SQL Server vs Postgres,
  light vs dark theme, Windows vs macOS widgets, region-specific tax rules).
- The products in a family are **designed to be used together** and must not be mixed.
- You want callers to depend on abstractions and not on concrete constructors.
- You expect to add a new family later and want the change confined to a new factory plus its
  products.

Do **not** use it when there is only one product. That is Factory Method or a simple factory. Do
not use it when the "family" is a set of unrelated services that merely share a namespace; ordinary
dependency injection is simpler.

## Abstract Factory vs. Related Patterns

- **Factory Method** creates one product and lets each product's creation vary independently;
  **Abstract Factory** creates a family and makes one choice for all of them. An abstract factory is
  often implemented with several factory methods.
- **Simple Factory** is a single class with a `switch` that returns one product; it has no product
  family and no abstract factory interface.
- **Builder** constructs one complex object step by step; **Abstract Factory** returns a set of
  already-constructed products in one call.
- **Bridge** separates an abstraction from one implementation dimension; **Abstract Factory**
  creates the implementations. They combine: a bridge's implementor can be produced by an abstract
  factory.
- **Strategy** swaps one algorithm the caller already holds; **Abstract Factory** decides which
  family of objects the caller receives.
- **Dependency Injection** supplies objects to a client; an abstract factory can be one of the
  things DI supplies, keeping the family selection in one composition-root decision.

## Notes on This Repo

The example is self-contained: no database, no NuGet packages beyond the .NET SDK, and no
environment configuration. It is intended to be read next to the other pattern demos in this
repository and run on its own.
