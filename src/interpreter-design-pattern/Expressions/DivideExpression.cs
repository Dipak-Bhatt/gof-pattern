namespace InterpreterDesignPattern;

/// <summary>
/// A nonterminal expression: left / right. Division by zero is rejected before it can
/// produce a meaningless allowance.
/// </summary>
public sealed class DivideExpression(IExpression left, IExpression right) : BinaryExpression(left, right)
{
    public override string Operator => "/";

    public override decimal Interpret(FormulaContext context)
    {
        var divisor = Right.Interpret(context);

        if (divisor == 0m)
        {
            throw new FormulaException("A formula cannot divide by zero.");
        }

        return Left.Interpret(context) / divisor;
    }
}
