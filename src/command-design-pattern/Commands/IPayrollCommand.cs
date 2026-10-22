using CommandDesignPattern.Models;
using CommandDesignPattern.Receivers;

namespace CommandDesignPattern.Commands;

public interface IPayrollCommand
{
    PayrollCommandType Type { get; }
    PayrollPeriod Period { get; }
    Task<CommandResult> ExecuteAsync(PayrollService payroll);
}
