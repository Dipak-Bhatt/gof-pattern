using CommandDesignPattern.Models;

namespace CommandDesignPattern.Commands;

public static class PayrollRunCommands
{
    public static IReadOnlyList<IPayrollCommand> For(PayrollPeriod period) =>
    [
        new LockPayrollPeriodCommand(period),
        new CalculateSalariesCommand(period),
        new GeneratePayslipsCommand(period),
        new EmailPayslipsCommand(period),
    ];
}
