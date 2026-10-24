# Memento Design Pattern

## Definition

The Memento pattern **captures an object's internal state and stores it outside the object**, so
the state can be restored later without exposing the object's implementation. The object that owns
the state creates and restores snapshots; a caretaker keeps them; the snapshot itself is opaque to
everyone except its originator.

This example is an **exam setup wizard**. The author changes the pass mark, duration, attempt
limit, section titles, section order and selected questions. Every change happens in place on one
draft, so undo cannot be implemented cleanly by reversing each field one at a time. Instead, the
wizard saves a snapshot before an edit and restores the whole snapshot when the author presses Undo.

## Structure

```text
┌────────────────────────┐
│ Client                 │
│ Program.cs             │
└───────────┬────────────┘
            │ edits
            ▼
┌────────────────────────┐   CreateMemento()   ┌──────────────────────────┐
│ Originator             │────────────────────>│ Memento                  │
│ ExamSetupDraft         │                     │ ExamSetupMemento         │
│                        │<────────────────────│                          │
└────────────────────────┘   Restore(memento)  │ private ExamSetupState   │
            ▲                                  └──────────────────────────┘
            │                                                   ▲
            │ Save / Undo / Redo                                │ stores
            │                                                   │
┌───────────┴────────────┐                                      │
│ Caretaker              │──────────────────────────────────────┘
│ ExamSetupHistory       │  never reads the snapshot's state
│ undo stack / redo stack│
└────────────────────────┘
```

The caretaker sees a memento's label and timestamp, but not the exam setup state inside it. Only
`ExamSetupDraft` can read a snapshot and copy it back into its own fields.

## Pattern Roles

| Role | Type | Responsibility |
|------|------|----------------|
| **Originator** | `ExamSetupDraft` | Owns the live state; creates a memento and restores from one |
| **Memento** | `ExamSetupMemento` | Holds an immutable snapshot; exposes only metadata to other roles |
| **Caretaker** | `ExamSetupHistory` | Keeps undo and redo stacks; never inspects a snapshot |
| **Client** | `Program.cs` | Edits the draft and asks the history to save, undo or redo |

## Project Layout

```text
memento-design-pattern/
├── Domain/
│   ├── ExamConfiguration.cs     — pass mark, duration and attempt limit
│   ├── ExamSection.cs           — a titled group of question ids
│   └── ExamSetupState.cs        — immutable value describing the whole draft
├── Memento/
│   └── ExamSetupMemento.cs      — MEMENTO: opaque stored snapshot
├── Originator/
│   └── ExamSetupDraft.cs        — ORIGINATOR: live state, CreateMemento / Restore
├── Caretaker/
│   └── ExamSetupHistory.cs      — CARETAKER: undo and redo stacks
├── Program.cs                   — client and demonstration
├── memento-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/memento-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```text
=== Memento Design Pattern — Exam Setup Undo ===

ORIGINATOR  ExamSetupDraft    owns the live exam configuration and sections
MEMENTO     ExamSetupMemento  opaque snapshot that only the originator can read
CARETAKER   ExamSetupHistory  stores snapshots and manages undo / redo

--- 1. A checkpoint is captured before editing ---
  pass=50%  duration=60m  attempts=3  sections=[Knowledge(2), Scenario(1)]  questions=3
  history: undo=1  redo=0

--- 2. The author changes the configuration in place ---
  pass=70%  duration=90m  attempts=4  sections=[Knowledge(2), Scenario(1)]  questions=3
  history: undo=1  redo=0

--- 3. The author changes the structure in place ---
  pass=70%  duration=90m  attempts=4  sections=[Knowledge(1), Case study(1), Behaviour(1)]  questions=3
  history: undo=2  redo=0

--- 4. Undo restores the saved checkpoint -> before structure edit ---
  pass=70%  duration=90m  attempts=4  sections=[Knowledge(2), Scenario(1)]  questions=3
  history: undo=1  redo=1

--- 5. Undo again returns to the opening state -> opened ---
  pass=50%  duration=60m  attempts=3  sections=[Knowledge(2), Scenario(1)]  questions=3
  history: undo=0  redo=2

--- 6. Redo returns to the configuration edit ---
  pass=70%  duration=90m  attempts=4  sections=[Knowledge(2), Scenario(1)]  questions=3
  history: undo=1  redo=1

--- 7. Redo returns to the structure edit ---
  pass=70%  duration=90m  attempts=4  sections=[Knowledge(1), Case study(1), Behaviour(1)]  questions=3
  history: undo=2  redo=0

--- 8. Undo reopens the redo branch -> previous state ---
  pass=70%  duration=90m  attempts=4  sections=[Knowledge(2), Scenario(1)]  questions=3
  history: undo=1  redo=1

--- 9. A new edit after undo clears the redo branch ---
  pass=80%  duration=75m  attempts=5  sections=[Knowledge(2), Scenario(1)]  questions=3
  history: undo=2  redo=0

=== Key Insight ===
Memento moves state capture and restoration into the object that owns the state.
The caretaker never reads a snapshot; it stores opaque mementos and hands them
back to the originator. Undo therefore uses one protocol — CreateMemento and
Restore — instead of the client trying to reverse every field it changed.
```

## How It Works

1. **The draft owns the state.** `ExamSetupDraft` holds a configuration and a list of sections.
   Its public `State` property returns immutable copies, so a caller cannot mutate the draft by
   changing a returned list.
2. **A snapshot is taken before an edit.** `ExamSetupHistory.Save` calls
   `draft.CreateMemento(label)`. The originator builds an `ExamSetupState` value and wraps it in an
   `ExamSetupMemento`.
3. **The caretaker stores it without reading it.** `ExamSetupHistory` pushes the memento onto its
   undo stack and clears the redo stack. It never touches the exam configuration or sections.
4. **The author edits in place.** `UpdateConfiguration`, `RenameSection`, `RemoveQuestion` and
   `AddSection` change the draft directly. No inverse operation is recorded.
5. **Undo captures the present, then restores the past.** The caretaker pushes the current state
   onto the redo stack, pops the latest undo memento and calls `draft.Restore(memento)`.
6. **Redo reverses the move.** The caretaker pushes the current state onto the undo stack, pops the
   redo memento and restores it.
7. **A new edit starts a new branch.** `Save` clears the redo stack, so an edit made after Undo
   discards the abandoned future.
8. **Snapshots are independent.** `ExamSetupDraft` copies every section's question list when it
   captures and restores state, so a later edit cannot change a stored memento.

## Why Memento Here?

A wizard edit touches several fields and collections. Reversing it with commands would mean one
inverse class per action and one more field to remember for every new attribute. Memento replaces
that with two operations on the owner:

- `CreateMemento()` — capture everything.
- `Restore(memento)` — put everything back.

That keeps future fields inside the class that declares them. Adding a new configuration field
changes `ExamSetupState`, `CreateMemento` and `Restore`, not the client, the history, or one
reverse-operation class per edit.

It also keeps the history honest: the caretaker cannot rebuild state from pieces, because it never
sees the pieces in the first place.

## Software Design Principles

| Principle | How this example applies it |
|-----------|-----------------------------|
| **Single Responsibility** | The draft owns state, the memento owns one saved state, the history owns undo/redo stacks, and `Program` owns the demonstration flow. |
| **Encapsulation** | `ExamSetupMemento` keeps its state private. The caretaker can only read the label and timestamp. |
| **Open/Closed** | A new action on the draft (for example "move section") needs no new undo class; it uses the same capture-and-restore protocol. |
| **Information Hiding** | The client never casts a memento or inspects its fields; it passes it back to the originator. |
| **Separation of Concerns** | The state transition lives in the originator, storage policy lives in the caretaker, and UI flow lives in the client. |

## Memento vs. Command

| Question | Memento | Command |
|----------|---------|---------|
| What is stored? | A previous **state** | An **operation** and its parameters |
| How is undo done? | Restore the whole snapshot | Execute an inverse operation |
| Best for | Editors, forms, wizards, drafts | Queues, logs, retries, scheduled work |
| Cost | One full snapshot per checkpoint | One small command per action |

They can be combined: a command can own the memento needed to undo its own effect.

## Real-World Analogy

A document editor does not reverse each keystroke when you press Undo. It keeps snapshots of the
document at meaningful points and swaps the current document for an earlier snapshot. The saved
snapshot contains formatting, text, images and cursor position even though the Undo button only
knows that it has "a previous version".

## When to Use

- You need undo, redo, rollback or "discard my edits" for an object with several fields.
- The state is edited in place and you want to capture it without exposing its internals.
- An edit is a transaction-like unit: a wizard step, a form save or a draft commit.
- You want an audit-friendly "before" snapshot alongside a change.
- The client should not know how to construct or copy the object's state.

## When Not to Use

- The state is huge or contains large binaries; full snapshots will waste memory.
- The object holds external resources (open streams, connections, file handles) that must not be
  duplicated into a snapshot.
- A single field changes and a direct inverse operation is simpler and cheaper.
- The state contains mutable references shared with the outside world; without deep copies, the
  snapshot is not really a snapshot.
- The undo history must survive a process restart but there is no persistence plan. In that case,
  make the caretaker persistence-aware or store the snapshot as a version row.

## Notes for Production Use

This demo keeps history in memory, which is right for a single wizard session. If undo must survive
a page reload or a service restart, persist the snapshot (or a compact equivalent) and reload it
into the caretaker. Keep the same rule: the stored form is opaque to the caretaker, and only the
originator may convert it back into live state.
