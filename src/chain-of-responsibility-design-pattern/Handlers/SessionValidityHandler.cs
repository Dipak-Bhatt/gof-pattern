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
