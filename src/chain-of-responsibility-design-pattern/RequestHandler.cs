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
