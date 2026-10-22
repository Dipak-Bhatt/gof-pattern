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
