namespace InterpreterDesignPattern;

/// <summary>
/// A terminal expression: a named value such as {Basic Salary}. The value is looked up in
/// the context every time the tree is interpreted.
/// </summary>
public sealed class VariableExpression(string name) : IExpression
{
    public string Name { get; } = name;

    public decimal Interpret(FormulaContext context) => context.Resolve(Name);

    public override string ToString() => $"{{{Name}}}";
}
