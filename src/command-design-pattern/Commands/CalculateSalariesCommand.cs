using CommandDesignPattern.Models;
using CommandDesignPattern.Receivers;

namespace CommandDesignPattern.Commands;

public sealed record CalculateSalariesCommand(PayrollPeriod Period) : IPayrollCommand
{
    public PayrollCommandType Type => PayrollCommandType.CalculateSalaries;

    public Task<CommandResult> ExecuteAsync(PayrollService payroll) =>
        payroll.CalculateSalariesAsync();
}
