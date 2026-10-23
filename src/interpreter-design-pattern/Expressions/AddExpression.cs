namespace InterpreterDesignPattern;

/// <summary>
/// A nonterminal expression: left + right.
/// </summary>
public sealed class AddExpression(IExpression left, IExpression right) : BinaryExpression(left, right)
{
    public override string Operator => "+";

    public override decimal Interpret(FormulaContext context)
        => Left.Interpret(context) + Right.Interpret(context);
}
