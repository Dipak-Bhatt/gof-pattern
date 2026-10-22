using System.Collections.Concurrent;
using CommandDesignPattern.Commands;

namespace CommandDesignPattern.Queued;

public sealed class InMemoryPayrollCommandQueue : IPayrollCommandQueue
{
    private readonly ConcurrentQueue<IPayrollCommand> _commands = new();

    public int Count => _commands.Count;

    public void Enqueue(IPayrollCommand command) => _commands.Enqueue(command);

    public bool TryDequeue(out IPayrollCommand? command) => _commands.TryDequeue(out command);
}
