# Composite Pattern — Employee Hierarchy

## Problem

The database stores the manager-employee hierarchy as a **closure table** — flat rows of `(ManagerRefId, SubordinateRefId, Depth)`. The application needs a **tree** where each node holds its direct reports and can answer questions like "how many people report to this person (at any depth)?" or "find employee X anywhere in my subtree."

## How It Works

```
Database (flat closure table)          Application (tree of IOrgNode)
┌─────────────────────────┐            ┌──────────────────────┐
│ ManagerRefId │ SubRefId │            │      IOrgNode       │
│     1       │    2     │──Builder──▶│  ┌─ DirectReports    │
│     1       │    3     │            │  ├─ TotalDescendants  │
│     2       │    4     │            │  └─ Find(id)         │
│     2       │    5     │            └──────────────────────┘
└─────────────────────────┘
```

`IOrgNode` is the Component. `ManagerNode` is the Composite (holds children). `EmployeeLeaf` is the Leaf (no children). The client treats them uniformly.

## Pattern Roles

| Role | Type |
|------|------|
| **Component** | `IOrgNode` |
| **Composite** | `ManagerNode` (holds `List<IOrgNode> DirectReports`) |
| **Leaf** | `EmployeeLeaf` (no children) |
| **Client** | `Program.cs` — calls `Find`, `Filter`, `TotalDescendantCount` without caring which node type |

## Running

```bash
cd src/composite-design-pattern
dotnet run
```

## Output

```
CEO total headcount: 5
Found employee 5: Dev2
Managers in tree: Alice (CEO), Bob (VP Eng), Carol (VP Sales)
```

## Key Insight

The Composite pattern lets a single employee and a whole team be treated the same way. `TotalDescendantCount` on a leaf returns `0`; on a manager it sums `1 + child.TotalDescendantCount` recursively. The client never needs to check `is ManagerNode` or `is EmployeeLeaf` — it just calls the method on `IOrgNode`.
