using CommandDesignPattern.Models;
using CommandDesignPattern.Receivers;

namespace CommandDesignPattern.Commands;

public sealed record LockPayrollPeriodCommand(PayrollPeriod Period) : IPayrollCommand
{
    public PayrollCommandType Type => PayrollCommandType.LockPeriod;

    public Task<CommandResult> ExecuteAsync(PayrollService payroll) =>
        payroll.LockPeriodAsync(Period);
}
