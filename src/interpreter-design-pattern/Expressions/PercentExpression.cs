namespace InterpreterDesignPattern;

/// <summary>
/// A nonterminal expression for a postfix percentage: 150% means 150 x 0.01.
/// </summary>
public sealed class PercentExpression(IExpression operand) : IExpression
{
    public IExpression Operand { get; } = operand;

    public decimal Interpret(FormulaContext context) => Operand.Interpret(context) * 0.01m;

    public override string ToString() => $"{Operand}%";
}
