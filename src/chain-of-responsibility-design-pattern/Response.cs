namespace ChainOfResponsibilityDesignPattern;

/// <summary>
/// What a handler writes when it decides to answer the request.
/// </summary>
public sealed class Response
{
    public int StatusCode { get; set; } = HttpStatus.Ok;

    public string? Body { get; set; }
}
