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
