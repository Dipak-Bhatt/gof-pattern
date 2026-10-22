using CommandDesignPattern.Commands;
using CommandDesignPattern.Models;
using CommandDesignPattern.Receivers;

namespace CommandDesignPattern.Immediate;

public sealed class ImmediatePayrollController(PayrollService payroll)
{
    public async Task SubmitAsync(PayrollPeriod period, int maxAttempts = 3)
    {
        Console.WriteLine($"[Controller] accepted payroll run for {period}; executing now...");

        foreach (var command in PayrollRunCommands.For(period))
            await ExecuteAsync(command, maxAttempts);
    }

    private async Task ExecuteAsync(IPayrollCommand command, int maxAttempts)
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
