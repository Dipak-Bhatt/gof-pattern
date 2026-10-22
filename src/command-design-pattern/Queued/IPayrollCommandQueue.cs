using CommandDesignPattern.Commands;

namespace CommandDesignPattern.Queued;

public interface IPayrollCommandQueue
{
    int Count { get; }
    void Enqueue(IPayrollCommand command);
    bool TryDequeue(out IPayrollCommand? command);
}
