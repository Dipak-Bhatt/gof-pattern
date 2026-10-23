namespace InterpreterDesignPattern;

/// <summary>
/// A nonterminal expression with two operands. Subclasses supply the operator and its
/// arithmetic; this base class holds the tree shape and a readable form of it.
/// </summary>
public abstract class BinaryExpression(IExpression left, IExpression right) : IExpression
{
    public IExpression Left { get; } = left;

    public IExpression Right { get; } = right;

    public abstract string Operator { get; }

    public abstract decimal Interpret(FormulaContext context);

    public override string ToString() => $"({Left} {Operator} {Right})";
}
