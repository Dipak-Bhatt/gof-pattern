using CommandDesignPattern.Models;
using CommandDesignPattern.Receivers;

namespace CommandDesignPattern.Commands;

public sealed record EmailPayslipsCommand(PayrollPeriod Period) : IPayrollCommand
{
    public PayrollCommandType Type => PayrollCommandType.EmailPayslips;

    public Task<CommandResult> ExecuteAsync(PayrollService payroll) =>
        payroll.EmailPayslipsAsync();
}
