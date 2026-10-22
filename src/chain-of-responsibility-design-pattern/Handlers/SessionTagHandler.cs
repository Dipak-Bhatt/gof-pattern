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
