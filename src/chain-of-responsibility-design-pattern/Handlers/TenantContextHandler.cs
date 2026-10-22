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
