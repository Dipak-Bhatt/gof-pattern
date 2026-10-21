# GoF Design Patterns

A collection of Gang of Four (GoF) design patterns implemented in C# (.NET 10). Each pattern is a standalone, runnable console project with a working example and README.

## Patterns

| Pattern | Location | Description |
|---------|----------|-------------|
| Adapter | `src/adapter-design-pattern/` | Lets two incompatible interfaces work together by wrapping one of them |
| Bridge | `src/bridge-design-pattern/` | Decouples abstraction from implementation so both can vary independently |
| Composite | `src/composite-design-pattern/` | Treats individual objects and compositions uniformly via a tree structure |
| Decorator | `src/decorator-design-pattern/` | Adds behaviour to an object by wrapping it in same-interface objects |
| Facade | `src/facade-design-pattern/` | Provides one simple entry point over a group of subsystem calls |
| Flyweight | `src/flyweight-design-pattern/` | Shares immutable state across many objects and passes the varying state in per call |
| Proxy | `src/proxy-design-pattern/` | Stands in for an object with the same interface — routes a call to one of several subjects, or guards access to a protected one |
| Singleton | `src/singleton-design-pattern/` | Ensures a class has exactly one instance with a global point of access |

## Structure

```
src/
├── adapter-design-pattern/     — Adapter pattern demo
├── bridge-design-pattern/      — Bridge pattern demo
├── composite-design-pattern/   — Composite pattern demo
├── decorator-design-pattern/   — Decorator pattern demo
├── facade-design-pattern/      — Facade pattern demo
├── flyweight-design-pattern/   — Flyweight pattern demo
├── proxy-design-pattern/       — Proxy pattern demo (Routing + Protection)
└── singleton-design-pattern/   — Singleton pattern demo
```

## Running

```bash
cd src/adapter-design-pattern
dotnet run

cd src/bridge-design-pattern
dotnet run

cd src/composite-design-pattern
dotnet run

cd src/decorator-design-pattern
dotnet run

cd src/facade-design-pattern
dotnet run

cd src/flyweight-design-pattern
dotnet run

cd src/proxy-design-pattern
dotnet run

cd src/singleton-design-pattern
dotnet run
```

## License

MIT
