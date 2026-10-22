# Chain of Responsibility Design Pattern

## Definition

The Chain of Responsibility pattern **passes a request along a chain of handlers until one of them answers it**. Each handler implements the same interface, holds a reference to the next handler, and decides on every call whether to:

- handle the request and stop the chain, or
- pass the request to the next handler.

The sender knows only the first handler. It does not know how many links exist, which one will answer, or whether any of them will. That decouples "who asks" from "who decides".

The pattern is a pipeline of independent checks, not one large method. Adding a check means adding one class and linking it in; the existing handlers do not change.

## Structure

```
┌──────────────────┐
│      Client      │   hands the request to the first handler
└────────┬─────────┘
         │
         ▼
┌──────────────────────────────┐
│  Handler (interface)         │
│  IRequestHandler             │◀──────────────┐
└──────────────▲───────────────┘               │
               │ implements                    │ implements
   ┌───────────┴────────────┐      ┌───────────┴──────────────────┐
   │ Base Handler           │      │ Concrete Handlers            │
   │ RequestHandler         │      │ TenantContextHandler         │
   │  - nextHandler         │      │ SessionValidityHandler       │
   │  - SetNext()           │      │ SessionTagHandler            │
   │  - ForwardAsync()      │      │ EndpointHandler (terminal)   │
   └────────────────────────┘      └──────────────────────────────┘
```

At runtime the client holds one `IRequestHandler` — the head of the chain. Every handler holds the next one behind the same interface. The request travels down the links until a handler writes a response and returns without forwarding.

## Project Layout

```
chain-of-responsibility-design-pattern/
├── IRequestHandler.cs                    — HANDLER interface
├── RequestHandler.cs                     — base handler (holds the next link)
├── RequestPipeline.cs                    — CLIENT: assembles and starts the chain
├── RequestContext.cs                     — the request plus the response being built
├── Response.cs                           — status code and body written by a handler
├── HttpStatus.cs                         — named status codes used by the demo
├── Handlers/
│   ├── TenantContextHandler.cs           — link 1: no tenant -> 302 or 401
│   ├── SessionValidityHandler.cs         — link 2: dead session -> 401
│   ├── SessionTagHandler.cs              — link 3: session changed in another tab -> 409
│   └── EndpointHandler.cs                — terminal link: executes the request -> 200
├── Program.cs                            — composition root and scenarios
└── README.md
```

## How to Run

```bash
cd src/chain-of-responsibility-design-pattern
dotnet run
```

## Output

```
=== Chain of Responsibility Pattern — request pipeline ===

1) Healthy request
   200: Endpoint executed.

2) No tenant — an API call
   401: Session expired or tenant context is missing.

3) No tenant — a browser navigation
   302: Redirecting to the sign-in page.

4) Expired session
   401: Session is not valid.

5) Session replaced in another tab
   409: This page is showing a session that was changed in another tab.
```

## The Code

### IRequestHandler.cs — Handler

```csharp
namespace ChainOfResponsibilityDesignPattern;

/// <summary>
/// HANDLER — the one contract every link implements. The client hands a request to the first
/// handler and never asks which link answered it.
/// </summary>
public interface IRequestHandler
{
    Task HandleAsync(RequestContext context);
}
```

### RequestHandler.cs — Base Handler

```csharp
namespace ChainOfResponsibilityDesignPattern;

/// <summary>
/// BASE HANDLER — holds the next link and forwards to it. A concrete handler derives from here,
/// makes its own decision, and calls <see cref="ForwardAsync"/> only when the request should
/// continue down the chain.
/// </summary>
public abstract class RequestHandler : IRequestHandler
{
    private IRequestHandler? _nextHandler;

    /// <summary>
    /// Links the next handler and returns it, so a pipeline can be built in one fluent line:
    /// <c>first.SetNext(second).SetNext(third)</c>.
    /// </summary>
    public RequestHandler SetNext(RequestHandler nextHandler)
    {
        ArgumentNullException.ThrowIfNull(nextHandler);
        _nextHandler = nextHandler;
        return nextHandler;
    }

    public abstract Task HandleAsync(RequestContext context);

    /// <summary>
    /// Passes the request to the next link. The last handler has no next link, so the call is a
    /// no-op and the chain simply ends.
    /// </summary>
    protected Task ForwardAsync(RequestContext context) =>
        _nextHandler is null ? Task.CompletedTask : _nextHandler.HandleAsync(context);
}
```

### RequestContext.cs

```csharp
namespace ChainOfResponsibilityDesignPattern;

/// <summary>
/// Everything a link in the chain needs to make its decision, plus the response it may write.
/// A handler reads the input and either fills <see cref="Response"/> or forwards the context
/// to the next handler.
/// </summary>
public sealed class RequestContext
{
    public Guid? TenantId { get; init; }

    public bool SessionIsValid { get; init; }

    public string? SessionTagSent { get; init; }

    public string? SessionTagCurrent { get; init; }

    public bool PrefersHtml { get; init; }

    public Response Response { get; } = new();
}
```

### Response.cs

```csharp
namespace ChainOfResponsibilityDesignPattern;

/// <summary>
/// What a handler writes when it decides to answer the request.
/// </summary>
public sealed class Response
{
    public int StatusCode { get; set; } = HttpStatus.Ok;

    public string? Body { get; set; }
}
```

### HttpStatus.cs

```csharp
namespace ChainOfResponsibilityDesignPattern;

/// <summary>
/// The status codes this demo uses, named so the handlers read like a sentence.
/// </summary>
public static class HttpStatus
{
    public const int Ok = 200;
    public const int Found = 302;
    public const int Unauthorized = 401;
    public const int Conflict = 409;
}
```

### Handlers/TenantContextHandler.cs — Link 1

```csharp
using ChainOfResponsibilityDesignPattern;

namespace ChainOfResponsibilityDesignPattern.Handlers;

/// <summary>
/// LINK 1 — refuses a request that has no tenant. A browser navigation is redirected to the
/// sign-in page; an API call gets a 401. Either way the chain stops here.
/// </summary>
public sealed class TenantContextHandler : RequestHandler
{
    public override Task HandleAsync(RequestContext context)
    {
        if (context.TenantId is null)
        {
            context.Response.StatusCode = context.PrefersHtml
                ? HttpStatus.Found
                : HttpStatus.Unauthorized;

            context.Response.Body = context.PrefersHtml
                ? "Redirecting to the sign-in page."
                : "Session expired or tenant context is missing.";

            return Task.CompletedTask;
        }

        return ForwardAsync(context);
    }
}
```

### Handlers/SessionValidityHandler.cs — Link 2

```csharp
using ChainOfResponsibilityDesignPattern;

namespace ChainOfResponsibilityDesignPattern.Handlers;

/// <summary>
/// LINK 2 — refuses a dead session with a 401. A tenant can be present while its session has
/// already expired, so this link exists separately from the tenant check.
/// </summary>
public sealed class SessionValidityHandler : RequestHandler
{
    public override Task HandleAsync(RequestContext context)
    {
        if (!context.SessionIsValid)
        {
            context.Response.StatusCode = HttpStatus.Unauthorized;
            context.Response.Body = "Session is not valid.";
            return Task.CompletedTask;
        }

        return ForwardAsync(context);
    }
}
```

### Handlers/SessionTagHandler.cs — Link 3

```csharp
using ChainOfResponsibilityDesignPattern;

namespace ChainOfResponsibilityDesignPattern.Handlers;

/// <summary>
/// LINK 3 — refuses a request from a browser tab that is still showing a session another tab has
/// replaced. The request is stopped with a 409 so a form rendered for one session can never be
/// saved into another.
/// </summary>
public sealed class SessionTagHandler : RequestHandler
{
    public override Task HandleAsync(RequestContext context)
    {
        var tagsDiffer =
            context.SessionTagSent is not null &&
            context.SessionTagCurrent is not null &&
            !string.Equals(context.SessionTagSent, context.SessionTagCurrent, StringComparison.Ordinal);

        if (tagsDiffer)
        {
            context.Response.StatusCode = HttpStatus.Conflict;
            context.Response.Body = "This page is showing a session that was changed in another tab.";
            return Task.CompletedTask;
        }

        return ForwardAsync(context);
    }
}
```

### Handlers/EndpointHandler.cs — Terminal Link

```csharp
using ChainOfResponsibilityDesignPattern;

namespace ChainOfResponsibilityDesignPattern.Handlers;

/// <summary>
/// LINK 4 — the terminal handler. Every link before it let the request through, so the work is
/// done and the chain ends with a 200.
/// </summary>
public sealed class EndpointHandler : RequestHandler
{
    public override Task HandleAsync(RequestContext context)
    {
        context.Response.StatusCode = HttpStatus.Ok;
        context.Response.Body = "Endpoint executed.";
        return Task.CompletedTask;
    }
}
```

### RequestPipeline.cs — Client

```csharp
namespace ChainOfResponsibilityDesignPattern;

/// <summary>
/// CLIENT — assembles the links into a chain and sends each request to the first one. The caller
/// knows only this pipeline; it does not know how many handlers there are or which one answers.
/// </summary>
public sealed class RequestPipeline
{
    private readonly IRequestHandler _firstHandler;

    public RequestPipeline(params RequestHandler[] handlers)
    {
        if (handlers.Length == 0)
        {
            throw new ArgumentException("A pipeline needs at least one handler.", nameof(handlers));
        }

        _firstHandler = handlers[0];

        for (var index = 0; index < handlers.Length - 1; index++)
        {
            handlers[index].SetNext(handlers[index + 1]);
        }
    }

    public Task HandleAsync(RequestContext context) => _firstHandler.HandleAsync(context);
}
```

### Program.cs — Composition Root

```csharp
using ChainOfResponsibilityDesignPattern;
using ChainOfResponsibilityDesignPattern.Handlers;

// The chain is assembled once, in the order the checks must run.
// Each handler decides whether to answer or to pass the request to the next one.
var pipeline = new RequestPipeline(
    new TenantContextHandler(),
    new SessionValidityHandler(),
    new SessionTagHandler(),
    new EndpointHandler());

Console.WriteLine("=== Chain of Responsibility Pattern — request pipeline ===\n");

await RunAsync(
    "1) Healthy request",
    pipeline,
    new RequestContext
    {
        TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        SessionIsValid = true,
        SessionTagSent = "tenant-a:ram",
        SessionTagCurrent = "tenant-a:ram"
    });

await RunAsync(
    "2) No tenant — an API call",
    pipeline,
    new RequestContext
    {
        SessionIsValid = true
    });

await RunAsync(
    "3) No tenant — a browser navigation",
    pipeline,
    new RequestContext
    {
        SessionIsValid = true,
        PrefersHtml = true
    });

await RunAsync(
    "4) Expired session",
    pipeline,
    new RequestContext
    {
        TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        SessionIsValid = false,
        SessionTagSent = "tenant-a:ram",
        SessionTagCurrent = "tenant-a:ram"
    });

await RunAsync(
    "5) Session replaced in another tab",
    pipeline,
    new RequestContext
    {
        TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        SessionIsValid = true,
        SessionTagSent = "tenant-a:ram",
        SessionTagCurrent = "tenant-b:sita"
    });

static async Task RunAsync(string title, RequestPipeline pipeline, RequestContext context)
{
    await pipeline.HandleAsync(context);

    Console.WriteLine(title);
    Console.WriteLine($"   {context.Response.StatusCode}: {context.Response.Body}");
    Console.WriteLine();
}
```

## Why This Example

Real web requests pass through a series of independent checks before the endpoint runs: is there a tenant, is the session alive, is the tab still showing the current session? Each check is a small class with one reason to exist, and the order is decided once in the composition root.

Adding a rate limit or a maintenance-window check means writing one more handler and placing it in the pipeline. No existing handler needs to know about it, and the endpoint still receives only requests that passed every link.

This is the same shape ASP.NET Core uses for middleware: each middleware receives a `RequestDelegate next` and calls it only when the request should continue. This demo makes the links explicit so the pattern is visible in plain C#.

## Real-World Analogy

A support ticket goes to the first-line agent. If they can solve it, they do. Otherwise they escalate to second line, and then to an engineer. Each person handles what is theirs and passes the rest on; the customer only knows they filed one ticket. The chain can grow or shrink without the customer changing how they ask.

## When to Use

- More than one object may handle a request, and the handler is not known in advance
- The set of handlers should be configured at runtime rather than fixed in a method
- You want to issue a request to one of several handlers without naming the receiver
- The checks are independent and can be added, removed or reordered
- A request should stop at the first handler that can answer it

## Chain of Responsibility vs. Related Patterns

- **Decorator** also wraps an object behind the same interface, but every decorator runs and the result flows back. A chain handler may stop the request entirely.
- **Composite** forwards a request to children as a tree. A chain forwards along a single line of handlers.
- **Strategy** picks one algorithm for the caller. A chain lets the request travel until one link accepts it.
- **Mediator** centralizes communication between objects; a chain has no central coordinator beyond the link references.

## Notes on This Repo

The scenario is a small HTTP request pipeline with no framework dependency. `RequestPipeline` plays the client: it links the handlers and starts the chain. The handlers themselves have no knowledge of the pipeline, only of the next `IRequestHandler`.
