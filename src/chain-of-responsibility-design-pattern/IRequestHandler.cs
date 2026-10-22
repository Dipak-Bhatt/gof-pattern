namespace ChainOfResponsibilityDesignPattern;

/// <summary>
/// HANDLER — the one contract every link implements. The client hands a request to the first
/// handler and never asks which link answered it.
/// </summary>
public interface IRequestHandler
{
    Task HandleAsync(RequestContext context);
}
