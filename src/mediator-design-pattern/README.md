# Mediator Design Pattern

## Definition

The Mediator pattern **centralises how a group of related objects interact**. Instead of each object holding references to the others and deciding who to call, every object talks to one mediator. The mediator knows the participants, the order of the interaction and the rules that apply to all of them.

This example models an **approval workflow**. Leave, Time, Travel, Advance, Expense Claim and Overtime are independent request modules (the colleagues). A caller that approves or rejects a request only talks to `ApprovalMediator`. The mediator finds the right module, records the approval levels, applies the decision and notifies the employee. The modules never reference each other, the tracker or the notifier.

Without the mediator, the caller repeats this workflow for every request type:

```csharp
switch (flowType)
{
    case Leave:         await leaveService.ApproveAsync(...); break;
    case Time:          await timeService.ApproveAsync(...); break;
    case Travel:        await travelService.ApproveAsync(...); break;
    // ...one branch per module, each with its own load/approve/reject API
}
```

With the mediator, the caller makes one call and knows no module:

```csharp
mediator.Approve(101, ApprovalFlowType.Leave, "approved by HR");
```

## Structure

```
┌──────────────────────────────┐
│  Client                      │
│  Program.cs                  │
└──────────────┬───────────────┘
               │ Approve(id, type, note) / Reject(id, type, note)
               ▼
┌──────────────────────────────┐
│  MEDIATOR                    │
│  ApprovalMediator            │
│  - finds the colleague       │
│  - records the levels        │
│  - applies the decision      │
│  - sends the notification    │
└───────┬──────────┬───────────┘
        │          │
        │          ├──────────────► IApprovalTracker
        │          │                (approval levels)
        │          │
        │          └──────────────► IApproverNotifier
        │                           (employee notification)
        │
        ▼
┌────────────────────────────────────────────────────────┐
│  COLLEAGUES (one per request type)                     │
│  Leave · Time · Travel · Advance · Expense Claim ·     │
│  Overtime                                              │
└────────────────────────────────────────────────────────┘
```

The colleagues do not know the tracker, the notifier or one another. The mediator is the only object that knows the whole workflow.

## Project Layout

```
mediator-design-pattern/
├── Models/
│   ├── ApprovalFlowType.cs          — the request types
│   ├── ApprovalRequest.cs           — the request and its status
│   └── ApprovalResult.cs            — success/failure result
├── Colleagues/
│   ├── IApprovalColleague.cs        — what every request module exposes
│   ├── ApprovalColleagueBase.cs     — shared in-memory behaviour
│   └── ApprovalColleagues.cs        — Leave, Time, Travel, Advance, Expense Claim, Overtime
├── Mediator/
│   ├── IApprovalMediator.cs         — the single entry point
│   ├── ApprovalMediator.cs          — MEDIATOR
│   ├── IApprovalTracker.cs          — approval-level contract
│   ├── ApprovalTracker.cs           — records levels
│   ├── IApproverNotifier.cs         — notification contract
│   └── ApproverNotifier.cs          — sends the decision
├── Program.cs                       — composition root and demo
├── mediator-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/mediator-design-pattern
dotnet run
```

## Output

```text
=== Mediator Pattern — Approval Workflow ===

MEDIATOR    ApprovalMediator
COLLEAGUES  Leave, Time, Travel, Advance, Expense Claim, Overtime
SHARED      record every approval level, apply the decision, notify the employee

--- Without the mediator, the caller owns the whole workflow ---
  switch (flowType)
  {
      case Leave:         await leaveService.ApproveAsync(...); break;
      case Time:          await timeService.ApproveAsync(...); break;
      case Travel:        await travelService.ApproveAsync(...); break;
      // ...one branch per module, each with its own load/approve/reject API
  }
  // and every branch still records levels and notifies the employee.

--- With the mediator, the caller makes one call ---

--- 1. Approve a leave request ---
  mediator: sending request 101 to the Leave colleague
    tracker: level 1/3 approved
    tracker: level 2/3 approved
    tracker: level 3/3 approved
    notify: Ram Bahadur — Leave request 101 is Approved
  result: OK — [Leave] Ram Bahadur's 3 days of casual leave approved and applied. Note: "approved by HR"

--- 2. Reject an expense claim ---
  mediator: sending request 301 to the Expense Claim colleague
    tracker: level 1/3 rejected (note: "receipt is missing")
    notify: Bikash Rai — ExpenseClaim request 301 is Rejected
  result: OK — [Expense Claim] Bikash Rai's NPR 4,500 client lunch rejected; nothing applied. Note: "receipt is missing"

--- 3. Approve a travel request ---
  mediator: sending request 201 to the Travel colleague
    tracker: level 1/3 approved
    tracker: level 2/3 approved
    tracker: level 3/3 approved
    notify: Hari Prasad — Travel request 201 is Approved
  result: OK — [Travel] Hari Prasad's field visit to Pokhara approved and applied. Note: "budget is available"

--- 4. A flow type with no registered colleague ---
  result: FAILED — No colleague is registered for Overtime.

--- 5. Add a new colleague at runtime ---

---    Approve the overtime request ---
  mediator: sending request 401 to the Overtime colleague
    tracker: level 1/3 approved
    tracker: level 2/3 approved
    tracker: level 3/3 approved
    notify: Anita Gurung — Overtime request 401 is Approved
  result: OK — [Overtime] Anita Gurung's 6 hours of overtime in October approved and applied. Note: "pre-approved by the manager"

=== Key Insight ===
The request modules never reference each other, the tracker or the notifier.
The caller never references a request module. Both sides talk only to the
mediator, which owns the order: find the colleague, record the levels, apply
the decision, then notify. Adding a request type is one colleague plus one
registration; no existing colleague and no caller has to change.
```

## How It Works

1. **`IApprovalColleague`** is the contract every request module implements. It exposes its `FlowType`, its `ModuleName`, one lookup and the two decision methods.
2. **`ApprovalColleagueBase`** stores each module's requests in memory and applies a decision to the request. A real module would update its own tables here; the mediator does not care how.
3. **`ApprovalMediator`** is the coordinator. It keeps a dictionary from `ApprovalFlowType` to colleague, so dispatch is one lookup rather than a growing `switch`.
4. **`IApprovalTracker`** owns the cross-cutting approval-level rule. The mediator calls it before the module applies the decision, so every request type follows the same order.
5. **`IApproverNotifier`** owns the employee notification. The request modules do not know it exists; the mediator wires the two sides together.
6. **The caller** (`Program.cs`) only knows `IApprovalMediator`. It never sees `LeaveApprovalColleague`, `ApprovalTracker` or `ApproverNotifier`.
7. **Adding a request type** means writing one colleague and registering it. No existing colleague, no caller and no mediator method changes.

## Why Mediator Fits Approval Workflows

Approval is a conversation between many modules that must follow the same protocol:

- **One protocol** — record the levels, apply the decision, notify. Putting it in each module duplicates the order and drifts over time.
- **One dispatch point** — the caller does not switch over request types or hold every module as a dependency.
- **Independent modules** — Leave does not reference Expense Claim; both only implement `IApprovalColleague`.
- **Open for extension** — a new request type (Overtime in the demo) registers a colleague at runtime without editing the mediator.
- **Testable** — a colleague can be tested alone, and the mediator can be tested with fake tracker and notifier implementations.

## Real-World Analogy

Think of an office reception desk. A visitor does not walk into every department looking for the right person. They tell the receptionist what they need; the receptionist knows which department handles it, which approval steps apply and who should be informed. The departments still do their own work and never need to know each other. If a new department opens, the receptionist gets one new contact — the existing departments do not change.

## When to Use

- Several related objects would otherwise hold references to one another, creating a web of dependencies
- The same interaction protocol must run for every participant
- You want one place to add, remove or reorder the participants
- A caller should not know which module handles a request
- You need to vary the interaction independently from the participants
- You do **not** need it when there are only two objects that talk to each other directly — the mediator would add a layer without removing any dependency
