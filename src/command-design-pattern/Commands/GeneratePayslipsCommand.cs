using CommandDesignPattern.Models;
using CommandDesignPattern.Receivers;

namespace CommandDesignPattern.Commands;

public sealed record GeneratePayslipsCommand(PayrollPeriod Period) : IPayrollCommand
{
    public PayrollCommandType Type => PayrollCommandType.GeneratePayslips;

    public Task<CommandResult> ExecuteAsync(PayrollService payroll) =>
        payroll.GeneratePayslipsAsync();
}
