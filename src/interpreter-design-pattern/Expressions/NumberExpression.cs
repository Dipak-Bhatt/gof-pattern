using System.Globalization;

namespace InterpreterDesignPattern;

/// <summary>
/// A terminal expression: a fixed number such as 150 or 0.5.
/// </summary>
public sealed class NumberExpression(decimal value) : IExpression
{
    public decimal Value { get; } = value;

    public decimal Interpret(FormulaContext context) => Value;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
