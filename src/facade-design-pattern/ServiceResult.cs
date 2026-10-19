namespace FacadeDesignPattern;

public class ServiceResult
{
    private ServiceResult(bool status, string message)
    {
        Status = status;
        Message = message;
    }

    public bool Status { get; }
    public string Message { get; }

    public static ServiceResult Success(string message) => new(true, message);
    public static ServiceResult Fail(string message) => new(false, message);

    public override string ToString() => $"{(Status ? "SUCCESS" : "FAILED")}: {Message}";
}
