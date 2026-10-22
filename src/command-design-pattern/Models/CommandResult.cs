namespace CommandDesignPattern.Models;

public sealed record CommandResult(bool Success, string Message)
{
    public static CommandResult Ok(string message) => new(true, message);
    public static CommandResult Retry(string message) => new(false, message);
}
