# Command Design Pattern

## Definition

The Command pattern **turns a request into a standalone object**. Instead of calling a receiver directly, the caller creates a command that describes what should happen, and an invoker decides when to run it. Because the request is an object, it can be queued, ordered, retried, logged, or handed to a different receiver without changing the caller.

This example models a **month-end payroll run** in an HRMS. `PayrollService` is the receiver that owns payroll state. The same four commands — lock the period, calculate salaries, generate payslips, email payslips — are executed in two different ways:

- **Design B — immediate execution:** `Controller → Command → PayrollService`
- **Design C — background queue:** `Controller → Command → Queue → Worker → PayrollService`

The operation is encapsulated in the command in both designs. The difference is **who executes it, and when**.

## Design B — Immediate Execution

```
┌──────────────────────────┐
│  ImmediatePayrollController │
│  (controller / invoker)  │
└────────────┬─────────────┘
             │ creates and executes
             ▼
┌──────────────────────────┐
│  IPayrollCommand         │
│  LockPayrollPeriod       │
│  CalculateSalaries       │
│  GeneratePayslips        │
│  EmailPayslips           │
└────────────┬─────────────┘
             │ ExecuteAsync(payroll)
             ▼
┌──────────────────────────┐
│  PayrollService          │
│  (receiver)              │
└──────────────────────────┘
```

The controller submits the run, executes each command immediately, and waits for the result. The caller stays blocked until the payroll run finishes.

## Design C — Background Queue

```
┌──────────────────────────┐
│  QueuedPayrollController │
│  (controller)            │
└────────────┬─────────────┘
             │ enqueues commands
             ▼
┌──────────────────────────┐
│  IPayrollCommandQueue    │
│  InMemoryPayrollCommandQueue │
└────────────┬─────────────┘
             │ TryDequeue
             ▼
┌──────────────────────────┐
│  PayrollWorker           │
│  (worker / invoker)      │
└────────────┬─────────────┘
             │ ExecuteAsync(payroll)
             ▼
┌──────────────────────────┐
│  PayrollService          │
│  (receiver)              │
└──────────────────────────┘
```

The controller only enqueues and returns immediately. A worker later pulls commands from the queue and executes them against the receiver. In production the in-memory queue would be replaced by a durable broker, so a process restart would not lose the run.

## Project Layout

```
command-design-pattern/
├── Program.cs                         — runs both designs
├── Models/
│   ├── PayrollPeriod.cs
│   └── CommandResult.cs
├── Commands/
│   ├── IPayrollCommand.cs
│   ├── PayrollCommandType.cs
│   ├── LockPayrollPeriodCommand.cs
│   ├── CalculateSalariesCommand.cs
│   ├── GeneratePayslipsCommand.cs
│   ├── EmailPayslipsCommand.cs
│   └── PayrollRunCommands.cs
├── Receivers/
│   └── PayrollService.cs
├── Immediate/
│   └── ImmediatePayrollController.cs  — Design B
├── Queued/
│   ├── IPayrollCommandQueue.cs
│   ├── InMemoryPayrollCommandQueue.cs
│   ├── QueuedPayrollController.cs
│   └── PayrollWorker.cs               — Design C
├── command-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/command-design-pattern
dotnet run
```

## Output

```text
=== Command Pattern — Design B: immediate execution ===

CHAIN  Controller -> Command -> PayrollService
The controller creates each command and executes it immediately.

[Controller] accepted payroll run for 2026-10; executing now...
  LockPeriod attempt 1: locked 2026-10 for edits
  CalculateSalaries attempt 1: calculated gross 255000 / net 216000 for 3 employees
  GeneratePayslips attempt 1: generated 3 payslips
  EmailPayslips attempt 1: mail server unavailable
    retrying...
  EmailPayslips attempt 2: emailed 3 payslips

Payroll run complete: locked=True, gross=255000, net=216000, payslips=3, emailed=3.

=== Command Pattern — Design C: background queue ===

CHAIN  Controller -> Command -> Queue -> Worker -> PayrollService
The controller only enqueues; a worker executes the commands later.

[Controller] enqueued 4 commands for 2026-10; returning immediately
Queue depth: 4; no work has run yet. locked=False, payslips=0

[Worker] picked up the queue and started processing
  LockPeriod attempt 1: locked 2026-10 for edits
  CalculateSalaries attempt 1: calculated gross 255000 / net 216000 for 3 employees
  GeneratePayslips attempt 1: generated 3 payslips
  EmailPayslips attempt 1: mail server unavailable
    retrying...
  EmailPayslips attempt 2: emailed 3 payslips
[Worker] queue empty; going idle

Payroll run complete: locked=True, gross=255000, net=216000, payslips=3, emailed=3.

=== Key Insight ===
Both designs use the same commands and the same receiver. Design B executes each
command inside the controller call. Design C puts the commands in a queue, so the
controller can return before any payroll work happens; the worker owns execution.
```

## How It Works

1. **`IPayrollCommand`** is the common type every payroll step shares. It exposes the command's `Type`, the `PayrollPeriod`, and `ExecuteAsync(PayrollService)`, so the operation is encapsulated in the command object itself.
2. **The command records** carry the parameters the step needs. Each is a small immutable object, which is why commands are easy to store, serialize or log.
3. **`PayrollRunCommands.For(period)`** builds the ordered run sheet. Both controllers reuse it, so the payroll steps cannot drift between the two designs.
4. **`PayrollService`** is the receiver. It owns the payroll state and is the only place that knows how to lock a period, calculate salaries, generate payslips or send email.
5. **`ImmediatePayrollController`** is Design B. It creates the commands and awaits each one immediately. The caller does not return until the run is finished.
6. **`QueuedPayrollController`** is Design C. It only enqueues commands and returns. **`PayrollWorker`** dequeues them and executes the same commands later, so the controller never touches the receiver.

The retry on `EmailPayslips` shows the practical value: a transient mail-server failure is retried without the caller or the other payroll steps knowing. Both designs share the same retry behavior, but only Design C lets the controller return before the work starts.

## Why Command Fits HRMS

Payroll is rarely one call. A run touches the period lock, the salary engine, document generation and email, and each step can fail independently. Command gives each step a name and an object, which makes the run:

- **Queueable** — Design C accepts a payroll request and returns before the run finishes
- **Retryable** — a failed email or PDF step can be retried without repeating the salary calculation
- **Auditable** — the command list is the run history: who asked for which period, and which step failed
- **Extensible** — a new step (bank file, tax filing, approval gate) is a new command plus a receiver method
- **Ordered** — the run sheet guarantees the period is locked before salaries are calculated
- **Testable** — a command can be exercised against a fake receiver without a controller or a queue

Choose **Design B** when the caller needs the result immediately, such as an admin action that previews a payroll calculation. Choose **Design C** when the work is long-running or must survive a process restart, such as a real month-end payroll run.

The same shape fits other HRMS workflows: employee offboarding (revoke access, stop salary, notify manager, archive), bulk attendance import, and scheduled report delivery.

## Real-World Analogy

A payroll officer does not personally calculate every salary, render every payslip and send every email. They assemble a run sheet: "lock October, calculate, generate payslips, send payslips." Each line is an instruction that a specialist can perform, and the officer can see which line is still pending. The run sheet is the command list; the specialists are the receivers.

In Design B the officer stands at each desk and waits for the specialist to finish. In Design C the officer drops the run sheet into an inbox; a back-office worker picks it up later. If the mail room is temporarily closed, only the "send payslips" line is retried — the salary calculation is not repeated.

## When to Use

- A request triggers **several ordered steps** that should not live in one controller method
- Work must be **queued or deferred**, such as a month-end payroll run or a nightly report
- A step can **fail transiently** and should be retried on its own
- You want a **log of requests** for audit, replay or support
- The caller should not know **which object performs** the operation
- You need **undo/redo**: each command can remember how to reverse itself
- You do **not** need it when the operation is a single synchronous call used once
