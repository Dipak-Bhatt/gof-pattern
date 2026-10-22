namespace CommandDesignPattern.Models;

public sealed record PayrollPeriod(int Year, int Month)
{
    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
