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
