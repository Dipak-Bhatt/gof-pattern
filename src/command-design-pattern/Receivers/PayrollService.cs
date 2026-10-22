using CommandDesignPattern.Models;

namespace CommandDesignPattern.Receivers;

public sealed class PayrollService(int employeeCount)
{
    private int _emailAttempts;

    public bool IsLocked { get; private set; }
    public decimal GrossTotal { get; private set; }
    public decimal NetTotal { get; private set; }
    public int PayslipsGenerated { get; private set; }
    public int PayslipsEmailed { get; private set; }

    public Task<CommandResult> LockPeriodAsync(PayrollPeriod period)
    {
        IsLocked = true;
        return Task.FromResult(CommandResult.Ok($"locked {period} for edits"));
    }

    public Task<CommandResult> CalculateSalariesAsync()
    {
        if (!IsLocked)
            return Task.FromResult(CommandResult.Retry("the period must be locked before calculation"));

        GrossTotal = employeeCount * 85000m;
        NetTotal = employeeCount * 72000m;

        return Task.FromResult(CommandResult.Ok(
            $"calculated gross {GrossTotal:0} / net {NetTotal:0} for {employeeCount} employees"));
    }

    public Task<CommandResult> GeneratePayslipsAsync()
    {
        if (NetTotal == 0)
            return Task.FromResult(CommandResult.Retry("run the salary calculation first"));

        PayslipsGenerated = employeeCount;
        return Task.FromResult(CommandResult.Ok($"generated {PayslipsGenerated} payslips"));
    }

    public Task<CommandResult> EmailPayslipsAsync()
    {
        if (PayslipsGenerated == 0)
            return Task.FromResult(CommandResult.Retry("generate the payslips first"));

        _emailAttempts++;
        if (_emailAttempts == 1)
            return Task.FromResult(CommandResult.Retry("mail server unavailable"));

        PayslipsEmailed = PayslipsGenerated;
        return Task.FromResult(CommandResult.Ok($"emailed {PayslipsEmailed} payslips"));
    }
}
