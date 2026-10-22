# Prototype Design Pattern

## Definition

The Prototype pattern **creates new objects by copying an existing object** instead of building them from scratch. The object that knows its own data exposes a `Clone()` operation, and every child in the object graph clones itself too. The client asks once and receives a fully independent copy without naming any concrete child type.

This example duplicates a **review form** made of sections, questions and answer options — a deep object graph where a shallow copy would be a bug, not a shortcut.

## Structure

```
┌──────────────────────┐
│   Client             │
│   ReviewFormDuplicator
└──────────┬───────────┘
           │ Clone()
           ▼
┌──────────────────────┐
│   Prototype (iface)  │
│   IPrototype<T>      │
└──────────┬───────────┘
           │ implements
           ▼
┌──────────────────────┐        clones        ┌──────────────────────┐
│   ReviewForm         │─────────────────────>│   ReviewSection      │
│   (aggregate root)   │                      │                      │
└──────────────────────┘                      └──────────┬───────────┘
                                                         │ clones
                                                         ▼
                                              ┌──────────────────────┐
                                              │   ReviewQuestion     │
                                              └──────────┬───────────┘
                                                         │ clones
                                                         ▼
                                              ┌──────────────────────┐
                                              │   AnswerOption       │
                                              └──────────────────────┘
```

Each concrete prototype returns a copy with `Id = 0`, so the copy can be saved as a new record. It also clears the audit fields and lifecycle flags that must not travel to a new form.

## Project Layout

```
prototype-design-pattern/
├── IPrototype.cs                  — PROTOTYPE interface (Clone)
├── ReviewForm.cs                  — CONCRETE PROTOTYPE (aggregate root)
├── ReviewSection.cs               — CONCRETE PROTOTYPE
├── ReviewQuestion.cs              — CONCRETE PROTOTYPE
├── AnswerOption.cs                — CONCRETE PROTOTYPE (leaf)
├── ReviewFormDuplicator.cs        — CLIENT: duplicates through Clone()
├── ManualFormDuplicator.cs        — the hand-written copy the pattern replaces
├── Program.cs                     — composition root and demonstration
└── README.md
```

## How to Run

```bash
cd src/prototype-design-pattern
dotnet run
```

## Output

```
=== Prototype Design Pattern — Duplicating a Review Form ===

PROTOTYPE          IPrototype<T>.Clone()
CONCRETE PROTOTYPE ReviewForm, ReviewSection, ReviewQuestion, AnswerOption
CLIENT             ReviewFormDuplicator.Duplicate(form)

--- Without the pattern: the caller copies every level by hand ---
  Copy of: Annual Review 2026 | sections=2, questions=2, options=4, formId=0, published=False
  ManualFormDuplicator knows all four types and resets every id and flag.

--- With the prototype: one call copies the whole graph ---
  Copy of: Annual Review 2026 | sections=2, questions=2, options=4, formId=0, published=False
  ReviewFormDuplicator calls source.Clone(); every child clones itself.

--- Why the copy must be deep: a shallow copy shares the children ---
  shallow copy section: 'MUTATED (shallow copy)'
  original section:     'MUTATED (shallow copy)'   <-- same object

--- Clone() returns an independent graph ---
  copy:     title='Copy of: Annual Review 2026', section='Goals (copy)', option='Met (copy)'
  original: title='Annual Review 2026', section='Goals', option='Met'
  same section object? False
  same option object?  False

=== Key Insight ===
Prototype moves duplication into the object that owns the data: each level knows how to
copy itself, so the client asks once and cannot miss a child collection. New fields stay
in the class that declares them instead of in a growing copy loop in the client.
Use it when creating an object from scratch is costly or when the graph is deep and
its concrete types should stay hidden from the caller.
```

## Software Design Principles

| Principle | How this example applies it |
|-----------|-----------------------------|
| **Open/Closed** | A new field or child collection is added to the class that owns it. The client's `Duplicate` call does not change. |
| **Single Responsibility** | Each class knows how to copy its own state. The client only decides that a copy is wanted and what to name it. |
| **Encapsulation** | The client never reaches into `_sections`, `_questions` or `_options`; cloning is the class's own responsibility. |
| **Liskov Substitution** | Any concrete prototype can stand behind `IPrototype<T>` because every implementation returns a fully usable `T`. |

## Shallow Copy vs Deep Copy

`MemberwiseClone()` copies value types and object references. For an aggregate, that means every copy shares its child lists — changing a section title on the copy changes the original too, as the demonstration shows. `Clone()` must therefore copy each child explicitly. That is the one rule the pattern asks you to get right.

## Real-World Analogy

Think of a contract template with standard clauses and fill-in fields. Instead of retyping the whole contract for each client, you copy the template, give the copy a new reference number and edit only the fields that differ. The original template stays untouched, and every copy starts complete.

## When to Use

- Creating an object from scratch is expensive: it needs a database read, a network call or a complex calculation.
- The object graph is deep and the client should not know its concrete child types.
- You need many variations of a configured object, and each variation starts from the same baseline.
- You want duplication logic to live with the data it copies, so adding a field cannot silently omit it from a copy.

## When Not to Use

- The object is simple and has no children; a constructor or a record `with` expression is clearer.
- Child objects are immutable and safely shared; a shallow copy may be correct and cheaper.
- The graph contains cycles or external resources (open streams, database connections) that must not be duplicated. Clone the data, not the resource.
