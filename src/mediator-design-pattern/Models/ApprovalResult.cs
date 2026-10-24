namespace MediatorDesignPattern.Models;

public sealed record ApprovalResult(bool Success, string Message)
{
    public static ApprovalResult Ok(string message) => new(true, message);

    public static ApprovalResult Fail(string message) => new(false, message);
}
