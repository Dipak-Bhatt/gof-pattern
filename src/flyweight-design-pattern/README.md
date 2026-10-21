# Flyweight Design Pattern

## Definition

The Flyweight pattern shares one immutable object across many uses instead of creating a copy per use. The shared object holds the intrinsic state — the part that is identical for every caller. The extrinsic state — the part that changes per request — is kept outside the shared object and passed in when it is needed.

This project uses an RLS (row-level security) scenario. The SQL depends only on which setting names exist; the row-level-security values change every request. So the SQL is built once per name-shape and shared, while the values are bound per request.

## Structure

```
┌──────────────────────────┐
│ Client                   │
│ Program.cs               │
└────────────┬─────────────┘
             │ Resolve(shape) / Compile(context)
             ▼
┌─────────────────────────────────────────────┐
│ FLYWEIGHT FACTORY                           │
│ RlsPrefix                                   │
│ - keys on RlsShape (ordered setting names)  │
│ - returns a shared CompiledPrefix           │
└────────────┬────────────────────────────────┘
             │ one object per shape
             ▼
┌─────────────────────────────────────────────┐
│ FLYWEIGHT (intrinsic state)                 │
│ CompiledPrefix                              │
│ - Sql                                       │
│ - ParameterNames                            │
│ - Render(values)                            │
│ immutable, shared, built once               │
└─────────────────────────────────────────────┘

Intrinsic key: RlsShape
- app.tenant_id, app.user_id
- shared by all requests with the same ordered names

Extrinsic state: RlsContext and RlsCommand
- values: tenant-0001, user-0001
- RlsCommand holds the shared prefix plus one request's values
```

## Project Layout

```
flyweight-design-pattern/
├── RlsShape.cs        — intrinsic key: the ordered setting names
├── RlsContext.cs      — shape plus values for one request
├── CompiledPrefix.cs  — the shared flyweight (intrinsic state)
├── RlsCommand.cs      — shared prefix plus values
├── RlsPrefix.cs       — the flyweight factory
├── Program.cs         — the demo
└── README.md
```

## How to Run

```bash
cd src/flyweight-design-pattern
dotnet run
```

## Output

```
=== Flyweight Pattern — Shared RLS Prefix ===

INTRINSIC   RlsShape + CompiledPrefix: setting names and compiled SQL, one per shape
EXTRINSIC   RlsContext / RlsCommand: the tenant and user values, per request

Requests: 1,001 two-setting contexts + 1 one-setting context
shared CompiledPrefix objects : 2
same shape reused by reference : True
different shape is distinct    : True

Shared SQL (intrinsic):
  SELECT set_config('app.tenant_id', @__rls_app_tenant_id_0, true), set_config('app.user_id', @__rls_app_user_id_1, true);

Same shared prefix, different values each request (extrinsic):
  SELECT set_config('app.tenant_id', @__rls_app_tenant_id_0, true), set_config('app.user_id', @__rls_app_user_id_1, true);  -- @__rls_app_tenant_id_0='tenant-0001', @__rls_app_user_id_1='user-0001'
  SELECT set_config('app.tenant_id', @__rls_app_tenant_id_0, true), set_config('app.user_id', @__rls_app_user_id_1, true);  -- @__rls_app_tenant_id_0='tenant-0002', @__rls_app_user_id_1='user-0002'

Resolving the same shape 10,000 times: 0 bytes allocated
(contexts and commands still allocate per request; the compiled prefix does not)

=== Key Insight ===
The SQL depends only on the shape (which setting names exist), so it is built once per
shape and shared. The values depend on the request, so they stay outside the shared
object and are bound on each call. Many contexts, two objects.
```
