using CommandDesignPattern.Commands;
using CommandDesignPattern.Receivers;

namespace CommandDesignPattern.Queued;

public sealed class PayrollWorker(
    IPayrollCommandQueue queue,
    PayrollService payroll,
    int maxAttempts = 3)
{
    public async Task RunAsync()
    {
        Console.WriteLine("[Worker] picked up the queue and started processing");

        while (queue.TryDequeue(out var command))
        {
            if (command is null)
                continue;

            await ExecuteAsync(command);
        }

        Console.WriteLine("[Worker] queue empty; going idle");
    }

    private async Task ExecuteAsync(IPayrollCommand command)
    {
        for (var attempt = 1; ; attempt++)
        {
            var result = await command.ExecuteAsync(payroll);
            Console.WriteLine($"  {command.Type} attempt {attempt}: {result.Message}");

            if (result.Success)
                return;

            if (attempt == maxAttempts)
                throw new InvalidOperationException(
                    $"{command.Type} failed after {maxAttempts} attempts.");

            Console.WriteLine("    retrying...");
        }
    }
}
