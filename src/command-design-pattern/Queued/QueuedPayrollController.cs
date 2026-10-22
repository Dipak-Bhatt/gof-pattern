using CommandDesignPattern.Commands;
using CommandDesignPattern.Models;

namespace CommandDesignPattern.Queued;

public sealed class QueuedPayrollController(IPayrollCommandQueue queue)
{
    public void Submit(PayrollPeriod period)
    {
        var commands = PayrollRunCommands.For(period);
        foreach (var command in commands)
            queue.Enqueue(command);

        Console.WriteLine(
            $"[Controller] enqueued {commands.Count} commands for {period}; returning immediately");
    }
}
