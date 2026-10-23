# Interpreter Design Pattern

## Definition

The Interpreter pattern **defines a grammar for a small language, represents each rule of the
grammar as a class, and evaluates a sentence by walking the resulting object tree**. Every
expression implements the same `Interpret(context)` operation, so the client can evaluate a whole
formula without checking which concrete node it is holding.

This example interprets an **allowance formula**:

```text
{Basic Salary}/{Monthly Hours}*{Units}*150%
```

The formula is stored as text, parsed once into an expression tree, and then interpreted for each
employee with that employee's values.

## The Formula Language

The grammar is deliberately small, which is what makes the pattern a good fit.

| Grammar rule | Example | Meaning |
|--------------|---------|---------|
| Number | `150` | A literal value |
| Variable | `{Basic Salary}` | A value supplied by the context |
| Add / Subtract | `{Units} + 2` | Terms combined with `+` or `-` |
| Multiply / Divide | `{Basic Salary} / {Monthly Hours}` | Primaries combined with `*` or `/` |
| Group | `({Units} + 2) * 3` | Parentheses override precedence |
| Percent | `150%` | Postfix percentage; `150%` means `150 * 0.01` |

Add and subtract bind least tightly, multiply and divide bind more tightly, and a percentage binds
to the value immediately before it. Parentheses override that ordering.

## Structure

```text
┌─────────────────────┐
│ Client              │
│ Program.cs          │
└──────────┬──────────┘
           │ FormulaParser.Parse(text)
           ▼
┌─────────────────────┐   builds   ┌──────────────────────────────┐
│ FormulaParser       │───────────>│ IExpression                  │
└─────────────────────┘            │ Interpret(context)           │
                                   └──────────────┬───────────────┘
                                                  │ implemented by
              ┌───────────────────────────────────┼──────────────────────────────┐
              ▼                                   ▼                              ▼
   ┌──────────────────────┐          ┌──────────────────────┐        ┌──────────────────────┐
   │ NumberExpression     │          │ VariableExpression   │        │ BinaryExpression     │
   │ TERMINAL             │          │ TERMINAL             │        │ NONTERMINAL          │
   └──────────────────────┘          └──────────────────────┘        │ Add / Subtract       │
                                                                     │ Multiply / Divide    │
                                                                     └──────────┬───────────┘
                                                                                │
                                                                     ┌──────────▼───────────┐
                                                                     │ PercentExpression    │
                                                                     │ NONTERMINAL (unary)  │
                                                                     └──────────────────────┘
```

The result of parsing is a tree. For example, the formula
`{Basic Salary}/{Monthly Hours}*{Units}*150%` becomes:

```text
*
├── *
│   ├── /
│   │   ├── {Basic Salary}
│   │   └── {Monthly Hours}
│   └── {Units}
└── %
    └── 150
```

Precedence is expressed by the shape of the tree, not by a flag or a separate evaluation loop.

## Pattern Roles

| Role | Type | Responsibility |
|------|------|----------------|
| **Abstract Expression** | `IExpression` | Declares `Interpret(context)` for every node |
| **Terminal Expression** | `NumberExpression`, `VariableExpression` | A leaf with no children |
| **Nonterminal Expression** | `BinaryExpression`, `AddExpression`, `SubtractExpression`, `MultiplyExpression`, `DivideExpression` | Combines two child expressions |
| **Nonterminal Expression** | `PercentExpression` | Wraps one child expression with the `%` rule |
| **Context** | `FormulaContext` | Supplies the values that variables resolve to |
| **Client** | `Program.cs` | Parses the formula and asks it to interpret |
| **Parser** | `FormulaParser` | Builds the tree from the stored text; not a GoF role, but needed in practice |

## Project Layout

```text
interpreter-design-pattern/
├── Expressions/
│   ├── IExpression.cs           — ABSTRACT EXPRESSION
│   ├── NumberExpression.cs      — TERMINAL EXPRESSION
│   ├── VariableExpression.cs    — TERMINAL EXPRESSION
│   ├── BinaryExpression.cs      — NONTERMINAL base
│   ├── AddExpression.cs         — NONTERMINAL: +
│   ├── SubtractExpression.cs    — NONTERMINAL: -
│   ├── MultiplyExpression.cs    — NONTERMINAL: *
│   ├── DivideExpression.cs      — NONTERMINAL: /
│   └── PercentExpression.cs     — NONTERMINAL: %
├── Parsing/
│   ├── FormulaContext.cs        — CONTEXT (variable values)
│   ├── FormulaException.cs      — malformed formula / missing value
│   └── FormulaParser.cs         — PARSER (text -> expression tree)
├── Program.cs                   — CLIENT and demonstration
├── interpreter-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/interpreter-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```text
=== Interpreter Design Pattern — Allowance Formula ===

CONTEXT        FormulaContext              values supplied for one employee
ABSTRACT EXPR  IExpression.Interpret(ctx) evaluates one node
TERMINAL       NumberExpression, VariableExpression
NONTERMINAL    Add, Subtract, Multiply, Divide, Percent
PARSER         FormulaParser.Parse(text)   builds the expression tree

Formula: {Basic Salary}/{Monthly Hours}*{Units}*150%
Parsed tree:
*
  *
    /
      {Basic Salary}
      {Monthly Hours}
    {Units}
  %
    150

Ram's allowance: 1,500.00
  (20,800 / 208) * 10 * 150% = 1,500.00

Same tree, interpreted for Sita:
  Sita's allowance: 3,150.00

Parentheses change the tree, so they change the result:
  {Units} + {Monthly Hours} * 2            =   426.00
  ({Units} + {Monthly Hours}) * 2          =   436.00

Malformed input is reported where the tree is built or interpreted:
  missing value:      The formula uses 'Bonus', but no value was supplied for it.
  invalid arithmetic: A formula cannot divide by zero.

=== Key Insight ===
Each grammar rule is its own class. A tree of those objects is the formula, and
Interpret() walks it with the supplied context. Adding an operator or a function
means adding a class, not editing one large evaluate() switch.
```

## How It Works

1. **Parse once.** `FormulaParser` reads the text and returns an expression tree. Each parsing
   method corresponds to one level of the grammar.
2. **Interpret many times.** `IExpression.Interpret` recursively evaluates the tree. A terminal
   node returns a number or looks up a variable; a nonterminal node evaluates its children and
   combines their results.
3. **Supply a context.** `FormulaContext` holds the values for one employee. Interpreting the same
   tree with another context evaluates the same formula for another employee.
4. **Handle errors close to their cause.** The parser rejects malformed text, `VariableExpression`
   rejects an unknown name, and `DivideExpression` rejects a zero divisor.
5. **Extend by adding a rule.** A new operator or function is a new expression class. Existing
   node classes do not change, and the parser gains one branch for the new token.

## Why Parse Once?

Parsing separates two concerns:

- **Syntax** — what the formula means (the tree).
- **Data** — which employee's values are used (the context).

If the formula is interpreted for 500 employees, the text is parsed once and the tree is evaluated
500 times. The tree also gives a natural place to validate a formula before storing it: a malformed
formula fails during parsing, and an unknown variable fails when the context is resolved.

## Software Design Principles

| Principle | How this example applies it |
|-----------|-----------------------------|
| **Open/Closed** | A new operator is a new expression class. Existing node classes do not change. |
| **Single Responsibility** | Parsing builds the tree; each expression evaluates its own rule; the context supplies values. |
| **Liskov Substitution** | Any node can stand behind `IExpression`, including a whole subtree. |
| **Composition over inheritance** | A tree is built by composing expression objects rather than by growing one `switch`. |
| **Encapsulation** | The client calls `Interpret`; it never inspects node types or manages evaluation stacks. |

## Real-World Analogy

A spreadsheet cell contains `=A1/B1*C1*150%`. The application parses the text once, then
recalculates the same tree whenever a referenced cell changes. The formula's structure stays the
same; only the values behind the names change. That is exactly what the parser and context do here.

## When to Use

- The domain has a small, recurring language: formulas, rules, filters or query expressions.
- The same expression is evaluated many times with different values.
- The grammar is stable enough to justify one class per rule.
- You want evaluation logic to be open for extension without editing a central `switch`.
- You need to validate the structure separately from evaluating it.

## When Not to Use

- The language is large or changes often; a parser generator or a library will be cheaper.
- The expression is evaluated only once and is trivial; a direct calculation is clearer.
- The grammar has only one or two rules; a class hierarchy would be overhead.
- You need high-performance numeric evaluation; a compiled expression or a stack machine may be
  a better fit after profiling.
