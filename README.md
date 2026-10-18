# GoF Design Patterns

A collection of Gang of Four (GoF) design patterns implemented in C# (.NET 10). Each pattern is a standalone, runnable console project with a working example and README.

## Patterns

| Pattern | Location | Description |
|---------|----------|-------------|
| Adapter | `src/adapter-design-pattern/` | Lets two incompatible interfaces work together by wrapping one of them |
| Bridge | `src/bridge-design-pattern/` | Decouples abstraction from implementation so both can vary independently |
| Singleton | `src/singleton-design-pattern/` | Ensures a class has exactly one instance with a global point of access |

## Structure

```
src/
├── adapter-design-pattern/     — Adapter pattern demo
├── bridge-design-pattern/      — Bridge pattern demo
└── singleton-design-pattern/   — Singleton pattern demo
```

## Running

```bash
cd src/adapter-design-pattern
dotnet run

cd src/bridge-design-pattern
dotnet run

cd src/singleton-design-pattern
dotnet run
```

## License

MIT
